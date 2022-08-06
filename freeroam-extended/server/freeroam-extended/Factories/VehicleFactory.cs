using System;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Async.CodeGen;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltVehicle : IVehicle
    {
        public IAltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
    }
    
    [AsyncEntity(typeof(IAltVehicle))]
    public partial class AltVehicle : Vehicle, IAltVehicle
    {
        public IAltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
        
        public AltVehicle(ICore server, uint model, Position position, Rotation rotation) : base(server, model, position, rotation)
        {
            SpawnTime = DateTime.Now;
        }
        
        public AltVehicle(ICore server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
            SpawnTime = DateTime.Now;
        }
    }
    
    public class AltVehicleFactory : IEntityFactory<IVehicle>
    {
        public IVehicle Create(ICore server, IntPtr playerPointer, ushort id)
        {
            return new AltVehicle(server, playerPointer, id);
        }
    }
}