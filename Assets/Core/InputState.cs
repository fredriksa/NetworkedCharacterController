using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class InputState : INetworkSerializable
    {
        public int Tick;
        public Vector3 MovementInput;
        public Vector3 RotationInput;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out MovementInput);
                reader.ReadValueSafe(out RotationInput);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(MovementInput);
                writer.WriteValueSafe(RotationInput);
            }
        }
    }
}
