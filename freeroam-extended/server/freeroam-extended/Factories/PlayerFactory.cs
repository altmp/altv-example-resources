using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Async.Elements.Entities;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltPlayer : IPlayer, IAsyncConvertible<IAltPlayer>
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; } 
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }
        public bool IsAdmin { get; set; }
        public int EventCount { get; set; }
    } 
    
    public partial class AltPlayer : AsyncPlayer, IAltPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; }
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }
        public bool IsAdmin { get; set; }
        public int EventCount { get; set; }

        public AltPlayer(ICore server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
            Vehicles = new List<AltVehicle>();
        }
        
        public new IAltPlayer ToAsync(IAsyncContext _) => this;
    }
    
    public class AltPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(ICore server, IntPtr playerPointer, ushort id)
        {
            return new AltPlayer(server, playerPointer, id);
        }
    }
}