using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Async.Elements.Entities;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltVehicle : IVehicle, IAsyncConvertible<IAltVehicle>
    {
        public IAltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
    }
    
    public partial class AltVehicle : AsyncVehicle, IAltVehicle
    {
        public IAltPlayer Owner { get; set; }
        public DateTime SpawnTime { get; set; }
        
        public AltVehicle(ICore server, uint model, Position position, Rotation rotation) : base(server, model, position, rotation)
        {
            SpawnTime = DateTime.Now;
        }
        
        public AltVehicle(ICore server, IntPtr nativePointer, uint id) : base(server, nativePointer, id)
        {
            SpawnTime = DateTime.Now;
        }
        
        public new IAltVehicle ToAsync(IAsyncContext _) => this;
    }
    
    public class AltVehicleFactory : IEntityFactory<IVehicle>
    {
        public IVehicle Create(ICore server, IntPtr playerPointer, uint id)
        {
            return new AltVehicle(server, playerPointer, id);
        }
    }
}