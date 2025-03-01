﻿namespace MapEditorReborn.Events.Handlers
{
    using EventArgs;
    using Exiled.Events.Extensions;

    using static Exiled.Events.Events;

    /// <summary>
    /// Schematic related events.
    /// </summary>
    public static class Schematic
    {
        public static event CustomEventHandler<SchematicSpawnedEventArgs> SchematicSpawned;

        public static event CustomEventHandler<ButtonInteractedEventArgs> ButtonInteracted;

        internal static void OnSchematicSpawned(SchematicSpawnedEventArgs ev) => SchematicSpawned.InvokeSafely(ev);

        internal static void OnButtonInteract(ButtonInteractedEventArgs ev) => ButtonInteracted.InvokeSafely(ev);
    }
}
