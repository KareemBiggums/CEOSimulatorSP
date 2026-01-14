using GTA;
using GTA.Math;
using GTA.UI;

namespace CEOSimulatorSP
{
    /// <summary>
    /// Handles office markers and interactions.
    /// </summary>
    public class OfficeManager
    {
        private readonly ConfigData _config;

        private const float MarkerDrawDistance = 50.0f;
        private const float InteractionDistance = 1.5f;

        public OfficeManager(ConfigData config)
        {
            _config = config;
        }

        public void DrawMarkers()
        {
            DrawMarker(_config.DeskMarker.ToVector3(), "Press ~INPUT_CONTEXT~ to use Executive Desk");
            DrawMarker(_config.AssistantMarker.ToVector3(), "Press ~INPUT_CONTEXT~ to use Assistant Desk");
            DrawMarker(_config.BoardroomChairMarker.ToVector3(), "Press ~INPUT_CONTEXT~ to start Board Meeting");
        }

        public OfficeInteraction GetInteractionTarget(Vector3 playerPosition)
        {
            if (playerPosition.DistanceToSquared(_config.DeskMarker.ToVector3()) < InteractionDistance * InteractionDistance)
            {
                return OfficeInteraction.Desk;
            }

            if (playerPosition.DistanceToSquared(_config.AssistantMarker.ToVector3()) < InteractionDistance * InteractionDistance)
            {
                return OfficeInteraction.Assistant;
            }

            if (playerPosition.DistanceToSquared(_config.BoardroomChairMarker.ToVector3()) < InteractionDistance * InteractionDistance)
            {
                return OfficeInteraction.Boardroom;
            }

            return OfficeInteraction.None;
        }

        private void DrawMarker(Vector3 position, string helpText)
        {
            var player = Game.Player.Character;
            if (player == null)
            {
                return;
            }

            var distance = player.Position.DistanceTo(position);
            if (distance > MarkerDrawDistance)
            {
                return;
            }

            World.DrawMarker(MarkerType.VerticalCylinder, position, Vector3.Zero, Vector3.Zero, new Vector3(0.6f, 0.6f, 0.4f), System.Drawing.Color.FromArgb(180, 0, 120, 200));
            if (distance <= InteractionDistance)
            {
                Screen.ShowHelpTextThisFrame(helpText);
            }
        }
    }

    public enum OfficeInteraction
    {
        None,
        Desk,
        Assistant,
        Boardroom
    }
}
