using Bang.Components;

namespace Pixpil.Messages
{
    public readonly struct AgentReleaseInputMessage : IMessage
    {
        public readonly int Button;
        public AgentReleaseInputMessage(int button)
        {
            Button = button;
        }
    }
}
