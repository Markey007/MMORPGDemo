﻿using GameFramework;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 角色技能
    /// </summary>
    public class ActorSkill : IActorSkill
    {
        protected ActorBase m_Owner = null;
        protected List<SkillTree> m_SkillPool = null;
        protected SkillTree m_CurrSkillTree;
        protected SkillTree m_MinCastDistSkillTree;
        protected List<SkillTree> m_ComboSkills =  new List<SkillTree>();
        protected int m_ComboIndex = 0;

        public ActorSkill(ActorBase owner)
        {
            m_Owner = owner;
            m_SkillPool = new List<SkillTree>();
            LoadSkill();

            float dist = 100000;
            for (int i = 0; i < m_SkillPool.Count; i++)
            {
                SkillTree skill = m_SkillPool[i];
                if (skill.CastDistance < dist && skill.CastDistance > 0)
                {
                    m_MinCastDistSkillTree = skill;
                    dist = skill.CastDistance;
                }

                if (skill.Pos == SkillPosType.Skill_0)
                {
                    m_ComboSkills.Add(skill);
                }
            }
        }

        public bool UseSkill(int id)
        {
            SkillTree skillTree = GetSkill(id);
            if (skillTree == null)
            {
                return false;
            }
            m_CurrSkillTree = skillTree;
            m_CurrSkillTree.Start();
            return true;
        }

        public bool UseSkill(SkillPosType pos)
        {
            SkillTree skillTree = GetSkill(pos);
            if (skillTree == null)
            {
                return false;
            }
            m_CurrSkillTree = skillTree;
            m_CurrSkillTree.Start();
            if (pos == SkillPosType.Skill_0)
            {
                m_ComboIndex = m_ComboIndex >= m_ComboSkills.Count - 1 ? 0 : ++m_ComboIndex;
            }
            return true;
        }

        public SkillTree GetSkill(SkillPosType pos)
        {
            for (int i = 0; i < m_SkillPool.Count; i++)
            {
                SkillTree skillTree = m_SkillPool[i];
                if (skillTree.Pos == pos)
                {
                    if (pos == SkillPosType.Skill_0)
                    {
                        return m_ComboSkills[m_ComboIndex];
                    }
                    return skillTree;
                }
            }
            return null;
        }

        public SkillTree GetSkill(int id)
        {
            for (int i = 0; i < m_SkillPool.Count; i++)
            {
                SkillTree skillTree = m_SkillPool[i];
                if (skillTree.Id == id)
                {
                    return skillTree;
                }
            }
            return null;
        }

        public float GetMinCastDistance()
        {
            return m_MinCastDistSkillTree == null ? 0 : m_MinCastDistSkillTree.CastDistance;
        }

        public SkillTree FindNextSkillByDist(float dist)
        {
            for (int i = 0; i < m_SkillPool.Count; i++)
            {
                SkillTree skillTree = m_SkillPool[i];
                if (skillTree.IsInCD())
                {
                    continue;
                }
                if (skillTree.CastDistance <= 0)
                {
                    return skillTree;
                }
                if (dist < skillTree.CastDistance)
                {
                    return skillTree;
                }
            }
            return null;
        }

        public SkillTree FindNextSkillByDist(Vector3 dest)
        {
            float dist = GlobalTools.GetHorizontalDistance(dest, m_Owner.Pos);
            return FindNextSkillByDist(dist);
        }

        public void Clear()
        {
            m_ComboIndex = 0;
            if (m_CurrSkillTree == null)
            {
                return;
            }
            m_CurrSkillTree.Break();
            m_CurrSkillTree = null;
        }

        private void LoadSkill()
        {
            string scriptName = m_Owner.Id.ToString();
            string assetName = AssetUtility.GetSkillScriptAsset(scriptName);
            string data = string.Empty;

                TextAsset asset = GameEntry.Resource.LoadAssetSync(assetName) as TextAsset;
            if (asset == null)
            {
                Log.Error("Skill script file is empty. file:{0}", m_Owner.Id.ToString());
                return;
            }
            data = asset.text;
            
            if (string.IsNullOrEmpty(data))
            {
                Log.Error("File content is null. file:{0}", assetName);
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data);

            XmlNode root = doc.FirstChild;
            if (root.Name.Equals("Actor") == false)
            {
                return;
            }
            XmlNode child = root.FirstChild;
            while (child != null)
            {
                if (child.Name.Equals("Skill"))
                {
                    XmlElement xe = child as XmlElement;
                    int id = XmlObject.ReadInt32(xe, "Id");
                    if (id > 0)
                    {
                        SkillTree skillTree = new SkillTree(id, m_Owner);
                        m_SkillPool.Add(skillTree);
                        skillTree.Load(xe);
                    }
                }
                child = child.NextSibling;
            }

            GameEntry.Resource.UnloadAsset(asset);
        }
    
    }
}
