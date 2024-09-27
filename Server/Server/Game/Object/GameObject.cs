﻿using Google.Protobuf.Protocol;
using System;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public GameRoom Room { get; set; }

		public ObjectInfo Info { get; set; } = new ObjectInfo();
		public PositionInfo PosInfo { get; private set; } = new PositionInfo();
		public StatInfo Stat { get; private set; } = new StatInfo();

		public float Speed
		{
			get { return Stat.Speed; }
			set { Stat.Speed = value; }
		}

		public int Hp
		{
			get { return Stat.Hp; }
			set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
		}

		public MoveDir Dir
		{
			get { return PosInfo.MoveDir; }
			set { PosInfo.MoveDir = value; }
		}

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}



		public GameObject()
		{
			Info.PosInfo = PosInfo;
			Info.StatInfo = Stat;
		}

		public virtual void Update()
		{

		}

		public Vector2Int CellPos
		{
			get
			{
				return new Vector2Int(PosInfo.PosX, PosInfo.PosY);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
			}
		}

		public Vector2Int GetFrontCellPos()
		{
			return GetFrontCellPos(PosInfo.MoveDir);
		}

		public Vector2Int GetFrontCellPos(MoveDir dir)
		{
			Vector2Int cellPos = CellPos;

			switch (dir)
			{
				case MoveDir.Up:
					cellPos += Vector2Int.up;
					break;
				case MoveDir.Down:
					cellPos += Vector2Int.down;
					break;
				case MoveDir.Left:
					cellPos += Vector2Int.left;
					break;
				case MoveDir.Right:
					cellPos += Vector2Int.right;
					break;
			}

			return cellPos;
		}

		public static MoveDir GetDirFromVec(Vector2Int dir)
		{
			if (dir.x > 0)
				return MoveDir.Right;
			else if (dir.x < 0)
				return MoveDir.Left;
			else if (dir.y > 0)
				return MoveDir.Up;
			else
				return MoveDir.Down;
		}

		public virtual void OnDamaged(int damage)
		{
			if (Room == null)
				return;

			Stat.Hp = Math.Max(Stat.Hp - damage, 0);

			// 변경된 체력을 모두에게 통지
			S_ChangeHp changePacket = new S_ChangeHp();
			changePacket.ObjectId = Id;
			changePacket.Hp = Stat.Hp;
			Room.Broadcast(changePacket);

			if (Stat.Hp <= 0)
			{
				OnDead();
			}
		}

		public virtual void OnDead()
		{
			if (Room == null)
				return;

			// 사망 정보를 모두에게 통지
			S_Die diePacket = new S_Die();
			diePacket.ObjectId = Id;
			Room.Broadcast(diePacket);

            // 방에서 제거
            GameRoom room = Room;
			room.LeaveGame(Id);

			// 즉시 리스폰 처리
			Stat.Hp = Stat.MaxHp;
			PosInfo.State = CreatureState.Idle;
			PosInfo.MoveDir = MoveDir.Down;
			PosInfo.PosX = 0;
			PosInfo.PosY = 0;

			room.EnterGame(this);
		}
	}
}
