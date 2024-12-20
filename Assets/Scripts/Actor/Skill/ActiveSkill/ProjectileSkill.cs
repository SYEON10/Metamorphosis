using System;
using System.Collections;
using System.Collections.Generic;
using Actor.Skill;
using UnityEngine;
using DG.Tweening;
using Fusion;
using UnityEngine.UI;

namespace Actor.Skill
{
    //Goal : Projectile을 만들어서 넣으면 스킬이 되도록!
    public class ProjectileSkill : ISkill
    {
        protected WrapBody _body;
        protected Transform _player; //스킬을 실행시킨 플레이어
        protected ActorController _actorController;

        //생성되지 않은 에셋(Resources)으로의 발사체
        public List<GameObject> projectileList { get; protected set; } = new List<GameObject>(5);
        //생성된 발사체
        //protected List<Projectile>[] projectileList = new List<Projectile>[100];

        public void Awake()
        {
            _stat = GetComponent<ActorStat>();
            _body = GetComponent<WrapBody>();
            _player = GetComponent<Transform>();
            _actorController = GetComponent<ActorController>();
        }

        public override void Activate()
        {
            StartCoroutine(CoolDown(_coolTime));
            _actorController._stateMachine.ChangeState(States.OnHit); //추후 공격모션으로 변경 + 각 스킬에 위임
        }

        //스킬 아이디, 발사할 발사체 아이디(index)
        protected Projectile Generate(int index)
        {
            if (!HasStateAuthority) return null;
            NetworkObject networkObject = Runner.Spawn(projectileList[index].GetComponent<NetworkObject>(), _player.position, Quaternion.identity, Runner.LocalPlayer);
            return networkObject.GetComponent<Projectile>();
        }
    }
}

