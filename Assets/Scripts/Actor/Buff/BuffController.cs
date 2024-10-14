using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    private LinkedList<IBuff> _buffList = new LinkedList<IBuff>();

    public void AddBuff(IBuff buff, float time) //지속시간 추가하기
    {
        _buffList.AddLast(buff);
        StartCoroutine(buff.StartBuff(time));
    }
}
