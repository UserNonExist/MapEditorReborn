﻿namespace MapEditorReborn.API.Features.Components.ObjectComponents
{
    using System.Collections.Generic;
    using Exiled.API.Enums;
    using Exiled.API.Features;
    using Features.Objects;
    using PlayerStatsSystem;
    using UnityEngine;

    using static API;

    /// <summary>
    /// Component added to RagdollSpawnPointObject. Is is used for easier idendification of the object and it's variables.
    /// </summary>
    public class RagdollSpawnPointComponent : MapEditorObject
    {
        /// <summary>
        /// Initializes the <see cref="RagdollSpawnPointComponent"/>.
        /// </summary>
        /// <param name="ragdollSpawnPoint">The <see cref="RagdollSpawnPointObject"/> to instantiate.</param>
        /// <returns>Instance of this compoment.</returns>
        public RagdollSpawnPointComponent Init(RagdollSpawnPointObject ragdollSpawnPoint)
        {
            Base = ragdollSpawnPoint;

            ForcedRoomType = ragdollSpawnPoint.RoomType != RoomType.Unknown ? ragdollSpawnPoint.RoomType : FindRoom().Type;
            UpdateObject();

            return this;
        }

        /// <summary>
        /// The config-base of the object containing all of it's properties.
        /// </summary>
        public RagdollSpawnPointObject Base;

        /// <inheritdoc cref="MapEditorObject.IsScalable"/>
        public override bool IsScalable => false;

        /// <inheritdoc cref="MapEditorObject.UpdateObject()"/>
        public override void UpdateObject()
        {
            OnDestroy();

            if (Random.Range(0, 101) > Base.SpawnChance)
                return;

            if (CurrentLoadedMap != null && string.IsNullOrEmpty(Base.Name) && CurrentLoadedMap.RagdollRoleNames.TryGetValue(Base.RoleType, out List<string> ragdollNames))
            {
                Base.Name = ragdollNames[Random.Range(0, ragdollNames.Count)];
            }

            RagdollInfo ragdollInfo;

            if (byte.TryParse(Base.DeathReason, out byte deathReasonId) && deathReasonId <= 22)
            {
                ragdollInfo = new RagdollInfo(Server.Host.ReferenceHub, new UniversalDamageHandler(-1f, DeathTranslations.TranslationsById[deathReasonId]), Base.RoleType, transform.position, transform.rotation, Base.Name, double.MaxValue);
            }
            else
            {
                ragdollInfo = new RagdollInfo(Server.Host.ReferenceHub, new CustomReasonDamageHandler(Base.DeathReason), Base.RoleType, transform.position, transform.rotation, Base.Name, double.MaxValue);
            }

            AttachedRagdoll = new Ragdoll(ragdollInfo, true);
        }

        private void OnDestroy()
        {
            AttachedRagdoll?.Delete();
            AttachedRagdoll = null;
        }

        /// <summary>
        /// The attached <see cref="Ragdoll"/>.
        /// </summary>
        public Ragdoll AttachedRagdoll;
    }
}
