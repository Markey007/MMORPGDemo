﻿using System;
using UnityEngine;
using UnityEngine.AI;

namespace GameMain
{
    public class AIPathFinding : IAIPathFinding
    {
        public event Action OnArriveEvent;

        private readonly IActor m_Owner;
        private readonly NavMeshAgent m_NavMeshAgent;
        private readonly NavMeshPath m_NavMeshPath;
        private GameObject m_GameObject;
        private Vector3 m_DestPosition;
        private Action m_OnFinished;

        public bool CheckReached()
        {
            if (!m_NavMeshAgent.enabled)
            {
                return false;
            }
            return m_NavMeshAgent.remainingDistance <= 1f && m_NavMeshAgent.remainingDistance > 0.01f;
        }

        public AIPathFinding(IActor owner)
        {
            m_Owner = owner;
            m_GameObject = m_Owner.EntityGo;
            this.m_NavMeshAgent = m_GameObject.GetOrAddComponent<NavMeshAgent>();
            this.m_NavMeshPath = new NavMeshPath();
            m_NavMeshAgent.enabled = false;
            m_NavMeshAgent.radius = m_Owner.Radius;
            m_NavMeshAgent.height = m_Owner.Height;
            m_NavMeshAgent.acceleration = 360;
            m_NavMeshAgent.angularSpeed = 360;
            m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        public void SetAgentEnable(bool enable)
        {
            this.m_NavMeshAgent.enabled = enable;
        }

        public void SetDestPosition(Vector3 dest)
        {
            m_DestPosition = dest;
            SetAgentEnable(true);
            this.m_NavMeshAgent.speed = m_Owner.Attrbute.GetValue(ActorAttributeType.Speed);
            m_NavMeshAgent.SetDestination(m_DestPosition);
        }

        public void Step()
        {
            if (m_NavMeshAgent.enabled == false)
            {
                return;
            }
            if (!m_Owner.CheckActorState(ActorStateType.IsAutoToMove))
            {
                return;
            }
            if (!CheckReached())
                return;

            OnArriveEvent?.Invoke();

            if (m_OnFinished == null)
                return;

            m_OnFinished();
            m_OnFinished = null;
        }

        public void Start()
        {

        }

        public void Clear()
        {

        }

        public void StopPathFinding()
        {
            if (m_NavMeshAgent.enabled == true)
            {
                m_NavMeshAgent.isStopped = true;
                SetAgentEnable(false);
            }
        }

        public bool CanReachPosition(Vector3 dest)
        {
            Vector3 position = GlobalTools.NavSamplePosition(dest);
            m_NavMeshAgent.enabled = true;
            m_NavMeshAgent.CalculatePath(position, m_NavMeshPath);
            if (m_NavMeshPath.status != NavMeshPathStatus.PathPartial)
            {
                return true;
            }
            return false;
        }

        public void SetOnFinished(Action callback)
        {
            m_OnFinished = callback;
        }
    }
}
