using Actor.Skill;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Boss.Skill
{
    // 광선형 스킬들의 기본형

    public class LaySkill : MonoBehaviour, BSkill
    {
        //임시 시리얼라이즈 필드
        [SerializeField] float _distance = 20.0f;
        [SerializeField] float _pre_width = 0.1f;
        [SerializeField] float _width = 1.0f;
        [SerializeField] Color _pre_color = new Color(1, 0, 0, 0.5f);
        [SerializeField] Color _color = new Color(0, 1, 0, 1.0f);
        LineRenderer _lineRenderer;
        private bool attacking = false;


        public void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void Activate()
        {
            StartCoroutine(ShootingLay(_lineRenderer));
        }

        IEnumerator ShootingLay(LineRenderer lr)
        {
            //전조 레이저 빔
            PreLay(lr);
            yield return new WaitForSeconds(1.0f);
            DeactiveLay(lr);

            //실제 레이저 빔
            AttackLay(lr);
            yield return new WaitForSeconds(0.5f);
            DeactiveLay(lr);
        }

        void PreLay(LineRenderer lr)
        {
            lr.positionCount = 2;
            lr.startColor = _pre_color;
            lr.endColor = _pre_color;
            lr.startWidth = _pre_width;
            lr.endWidth = _pre_width;

            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, new Vector3(this.transform.position.x + this._distance, this.transform.position.y, this.transform.position.z));
        }

        void AttackLay(LineRenderer lr)
        {
            lr.positionCount = 2;
            lr.startColor = _color;
            lr.endColor = _color;
            lr.startWidth = _width;
            lr.endWidth = _width;

            lr.SetPosition(0, this.transform.position);
            lr.SetPosition(1, new Vector3(this.transform.position.x + this._distance, this.transform.position.y, this.transform.position.z));

            attacking = true;
        }

        void DeactiveLay(LineRenderer lr)
        {
            for(int i = 0; i < lr.positionCount; i++)
            {
                lr.SetPosition(i, Vector3.zero);
            }
            lr.positionCount = 0;
            attacking = false;
        }
    }
}
