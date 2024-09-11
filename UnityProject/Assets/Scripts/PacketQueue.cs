using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����Ƽ�� ���� �����忡���� ������ ������ �� �ֱ� ������
/// ��Ŷ�� ó���� ���� �����忡�� �ؾ��Ѵ�. ������ ��Ŷ�� ť�� ��Ƶΰ� ���� �����忡�� ó���ϵ��� �Ѵ�.
/// </summary>
public class PacketQueue
{
    public static PacketQueue Instance { get; } = new PacketQueue();

    Queue<IPacket> packetQueue = new Queue<IPacket>();
    object lockObj = new object();



    public void Push(IPacket packet)
    {
        lock (lockObj)
        {
            packetQueue.Enqueue(packet);
        }
    }

    public IPacket Pop()
    {
        lock (lockObj)
        {
            if (packetQueue.Count == 0)
                return null;

            return packetQueue.Dequeue();
        }
    }

    public List<IPacket> PopAll()
    {
        List<IPacket> list = new List<IPacket>();

        lock (lockObj)
        {
            while (packetQueue.Count > 0)
                list.Add(packetQueue.Dequeue());
        }

        return list;
    }
}
