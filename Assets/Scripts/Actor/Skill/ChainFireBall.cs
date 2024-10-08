using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Actor.Skill
{
    public class ChainFireBall : ProjectileSkill
    {
        private BuffController _buff;

        public new void Awake()
        {
            base.Awake();
            _buff = GetComponent<BuffController>();
            _icon = Resources.Load<Sprite>("SkillIcon/ChainFireBall");
            _coolTime = 4f;
            
        }
        //Activate에서 can use 막기
        public override void Activate()
        {
            if (!_canUse) return;
            base.Activate();
            int id = GetSkillId();
            projectileList[id] = new List<Projectile>();
            StartCoroutine(Chain(id));
            _buff.AddBuff(new MaxHPUpBuff(_stat));
        }
        
        IEnumerator Chain(int id)
        {
            Vector3 primaryPosition = _stat.transform.position;
            Vector3 primaryDirection = _body.currentDirectionX;
            for (int i = 0; i < 3; i++)
            {
                Generate(id, 0);
                projectileList[id][i].Init(_stat, primaryDirection, primaryPosition, new Vector3(1f, 0.7f, 0f), 1.2f, new Vector3(10f, 0f, 0f));
                projectileList[id][i].Fire();
                yield return new WaitForSeconds(0.7f);
            }
        }
    }
}