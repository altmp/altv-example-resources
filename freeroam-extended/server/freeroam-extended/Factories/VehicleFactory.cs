using System;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended.Factories
{
    public class AltVehicle : Vehicle
    {
        public AltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; init; }
        
        public AltVehicle(IServer server, uint model, Position position, Rotation rotation) : base(server, model, position, rotation)
        {
            SpawnTime = DateTime.Now;
        }
        
        public AltVehicle(IServer server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
            SpawnTime = DateTime.Now;
        }
    }
    
    public class AltVehicleFactory : IEntityFactory<IVehicle>
    {
        public IVehicle Create(IServer server, IntPtr playerPointer, ushort id)
        {
            return new AltVehicle(server, playerPointer, id);
        }
    }
}