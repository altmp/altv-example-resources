using System;
using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Async.CodeGen;
using AltV.Net.Elements.Entities;

namespace Freeroam_Extended.Factories
{
    public partial interface IAltPlayer : IPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; }
    } 
    
    [AsyncEntity(typeof(IAltPlayer))]
    public partial class AltPlayer : Player, IAltPlayer
    {
        public IList<AltVehicle> Vehicles { get; set; }
        public DateTime LastVehicleSpawn { get; set; }

        public AltPlayer(IServer server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
            Vehicles = new List<AltVehicle>();
        }
    }
    
    public class AltPlayerFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(IServer server, IntPtr playerPointer, ushort id)
        {
            return new AltPlayer(server, playerPointer, id);
        }
    }
}