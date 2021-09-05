﻿namespace MapEditorReborn.API
{
    using System;
    using System.Collections.Generic;
    using Exiled.API.Features;
    using Exiled.API.Features.Items;
    using Exiled.CustomItems.API.Features;
    using MEC;
    using UnityEngine;

    using Random = UnityEngine.Random;

    /// <summary>
    /// Used for handling ItemSpawnPoint's spawning items.
    /// </summary>
    public class ItemSpawnPointComponent : MapEditorObject
    {
        /// <summary>
        /// The name of <see cref="ItemType"/> or <see cref="CustomItem"/>.
        /// </summary>
        public string ItemName = "KeycardJanitor";

        /// <summary>
        /// The chance for spawning a item.
        /// </summary>
        public int SpawnChance = 100;

        /// <summary>
        /// The number of spawned items.
        /// </summary>
        public uint NumberOfItems = 1;

        /// <summary>
        /// The list of <see cref="Pickup"/> items spawned by the <see cref="ItemSpawnPointObject"/>. May be <see langword="null"/>.
        /// </summary>
        public List<Pickup> AttachedPickups = new List<Pickup>();

        /// <summary>
        /// Initializes the <see cref="ItemSpawnPointComponent"/>.
        /// </summary>
        /// <param name="itemSpawnPoint"></param>
        public void Init(ItemSpawnPointObject itemSpawnPoint = null)
        {
            if (itemSpawnPoint != null)
            {
                ItemName = itemSpawnPoint.Item;
                SpawnChance = itemSpawnPoint.SpawnChance;
                NumberOfItems = itemSpawnPoint.NumberOfItems;
            }

            if (Random.Range(0, 101) > SpawnChance)
                return;

            if (Enum.TryParse(ItemName, out ItemType parsedItem))
            {
                for (int i = 0; i < NumberOfItems; i++)
                {
                    AttachedPickups.Add(new Item(parsedItem).Spawn(gameObject.transform.position, gameObject.transform.rotation));
                }
            }
            else
            {
                for (int i = 0; i < NumberOfItems; i++)
                {
                    Timing.RunCoroutine(SpawnCustomItem());
                }
            }
        }

        private IEnumerator<float> SpawnCustomItem()
        {
            yield return Timing.WaitUntilTrue(() => Round.IsStarted);

            if (CustomItem.TrySpawn(ItemName, gameObject.transform.position, out Pickup customItem))
            {
                customItem.Rotation = gameObject.transform.rotation;
                AttachedPickups.Add(customItem);
            }
        }

        private void OnDestroy()
        {
            foreach (Pickup pickup in AttachedPickups)
            {
                pickup.Destroy();
            }
        }
    }
}