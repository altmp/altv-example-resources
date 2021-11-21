using System;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Async.CodeGen;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltVehicle : IVehicle
    {
        public AltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
    }
    
    [AsyncEntity(typeof(IAltVehicle))]
    public partial class AltVehicle : Vehicle, IAltVehicle
    {
        public AltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
        
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