﻿using Google.Protobuf.Protocol;
using Server.DB;

namespace Server.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if(player == null)
                return;

            player.HandleEquipItem(equipPacket);
        }
    }
}
