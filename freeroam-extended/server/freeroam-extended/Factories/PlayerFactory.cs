using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Async.Elements.Entities;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Freeroam_Extended.Clothes;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltPlayer : IPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; } 
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }
        public bool IsAdmin { get; }
        
        public int EventCount { get; }
        void ResetEventCount();
        void IncrementEventCount();
        
        public string CloudID { get; set; }
        public long OutfitHash { get; set; }
        public uint Sex { get; }
        public PlayerData Data { get; set; }
        public string Serialize();
    }

    public class PlayerData
    {
        public bool Operator { get; set; }
        public bool Muted { get; set; }
    }

    public partial class AltPlayer : AsyncPlayer, IAltPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; }
        public bool GhostMode { get; set; }
        public bool EnableWeaponUsage { get; set; }
        public bool DmMode { get; set; }
        public bool NoClip { get; set; }

        public bool IsAdmin => Data.Operator;
        
        public string CloudID { get; set; }
        public long OutfitHash { get; set; }
        public uint Sex => this.Model switch
        {
            (uint)PedModel.FreemodeMale01 => 0,
            (uint)PedModel.FreemodeFemale01 => 1,
            _ => 2
        };

        private int _eventCount;
        public int EventCount => _eventCount;
        
        public void ResetEventCount()
        {
            Interlocked.Exchange(ref _eventCount, 0);
        }
        
        public void IncrementEventCount()
        {
            Interlocked.Increment(ref _eventCount);
        }

        public PlayerData Data { get; set; } = new();

        public AltPlayer(ICore server, IntPtr nativePointer, uint id) : base(server, nativePointer, id)
        {
            Vehicles = new List<AltVehicle>();
        }

        public string Serialize()
        {
            return this.Name + " [" + this.Id + "]";
        }
    }
    
    public class AltPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(ICore core, IntPtr entityPointer, uint id)
        {
            return new AltPlayer(core, entityPointer, id);
        }
    }
}