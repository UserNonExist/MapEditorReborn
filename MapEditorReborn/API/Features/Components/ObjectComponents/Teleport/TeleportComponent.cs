﻿namespace MapEditorReborn.API.Features.Components.ObjectComponents
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Events.EventArgs;
    using Exiled.API.Features;
    using Extensions;
    using Mirror;
    using UnityEngine;

    using Random = UnityEngine.Random;

    /// <summary>
    /// The component added to both child teleport object that were spawnwed by <see cref="TeleportControllerComponent"/>.
    /// </summary>
    public class TeleportComponent : MapEditorObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeleportComponent"/> class.
        /// </summary>
        /// <param name="chance">The required <see cref="TeleportControllerComponent"/>.</param>
        /// <param name="spawnIndicator">A value indicating whether the indicator should be spawned.</param>
        /// <returns>The initialized <see cref="TeleportComponent"/> instance.</returns>
        public TeleportComponent Init(float chance, bool spawnIndicator = false)
        {
            Chance = chance;

            if (transform.parent.TryGetComponent(out TeleportControllerComponent controller))
                Controller = controller;

            if (transform.parent.TryGetComponent(out TeleportControllerComponent teleportControllerComponent) && TryGetComponent(out BoxCollider boxCollider))
            {
                Controller = teleportControllerComponent;
                boxCollider.isTrigger = true;

                if (spawnIndicator)
                    UpdateObject();

                return this;
            }

            return null;
        }

        /// <summary>
        /// The teleport chance.
        /// </summary>
        public float Chance;

        /// <summary>
        /// The controller of this teleport.
        /// </summary>
        public TeleportControllerComponent Controller;

        /// <summary>
        /// Gets a value indicating whether the teleport is an entrance.
        /// </summary>
        public bool IsEntrance => Chance == -1f;

        /// <inheritdoc cref="MapEditorObject.UpdateObject"/>
        public override void UpdateObject() => this.UpdateIndicator();

        private void OnTriggerEnter(Collider collider)
        {
            if (!CanBeTeleported(collider, out GameObject gameObject))
                return;

            Player player = Player.Get(gameObject);
            Vector3 destination = IsEntrance ? Choose(Controller.ExitTeleports).transform.position : Controller.EntranceTeleport.transform.position;

            TeleportingEventArgs ev = new TeleportingEventArgs(this, IsEntrance, gameObject, player, destination);
            Events.Handlers.Teleport.OnTeleporting(ev);

            gameObject = ev.TeleportedObject;
            player = ev.TeleportedPlayer;
            destination = ev.Destination;

            if (!ev.IsAllowed)
                return;

            Controller.LastUsed = DateTime.Now;

            if (player != null)
            {
                player.Position = destination;
            }
            else
            {
                gameObject.transform.position = destination;
            }
        }

        private bool CanBeTeleported(Collider collider, out GameObject gameObject)
        {
            gameObject = null;

            bool flag = (IsEntrance || Controller.Base.BothWayMode) &&
                !CullingComponents.CullingComponent.CullingColliders.Contains(collider) &&
                (!Map.IsLczDecontaminated || !Controller.Base.LockOnEvent.HasFlagFast(LockOnEvent.LightDecontaminated)) &&
                (!Warhead.IsDetonated || !Controller.Base.LockOnEvent.HasFlagFast(LockOnEvent.WarheadDetonated)) &&
                DateTime.Now >= (Controller.LastUsed + TimeSpan.FromSeconds(Controller.Base.TeleportCooldown));

            if (!flag)
                return false;

            gameObject = collider.GetComponentInParent<NetworkIdentity>()?.gameObject;

            switch (gameObject.tag)
            {
                case "Player":
                    return Controller.Base.TeleportFlags.HasFlagFast(TeleportFlags.Player);
                case "Pickup":
                    return Controller.Base.TeleportFlags.HasFlagFast(TeleportFlags.Pickup);
                default:
                    return (gameObject.name.Contains("Projectile") && Controller.Base.TeleportFlags.HasFlagFast(TeleportFlags.ActiveGrenade)) ||
                           (gameObject.name.Contains("Pickup") && Controller.Base.TeleportFlags.HasFlagFast(TeleportFlags.Pickup));
            }
        }

        private static TeleportComponent Choose(List<TeleportComponent> teleports)
        {
            float total = 0;

            foreach (var elem in teleports)
            {
                total += elem.Chance;
            }

            float randomPoint = Random.value * total;

            for (int i = 0; i < teleports.Count; i++)
            {
                if (randomPoint < teleports[i].Chance)
                {
                    return teleports[i];
                }
                else
                {
                    randomPoint -= teleports[i].Chance;
                }
            }

            return teleports[teleports.Count - 1];
        }

        private void OnDestroy() => Controller.ExitTeleports.Remove(this);
    }
}
