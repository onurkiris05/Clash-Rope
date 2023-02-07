using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using VP.Nest.CameraControl;

namespace _Main.Scripts.Managers
{
    public class BattleManager : MonoBehaviour
    {
        [Header("General Components")]
        [SerializeField] EnemyManager enemyManager;
        [SerializeField] CameraManager cameraManager;

        [Header("Deploy Settings")] [SerializeField]
        Transform deployPosMin;
        [SerializeField] Transform deployPosMax;
        [SerializeField] int rowSize;
        [SerializeField] int rowCount;
        [SerializeField] float offsetBetweenRows;
        [SerializeField] float zigzagRowOffset;
        [SerializeField] float deploySpeed;

        Queue<Vector3> deployPositions = new Queue<Vector3>();

        int x;
        int currentIndex;

        void Start()
        {
            SetSquareFormation();
        }

        public void Set()
        {
            SetSquareFormation();
            currentIndex = 0;
        }

        public void ToggleBattleCamera()
        {
            StartCoroutine(ProcessBattleCamera());
        }

        IEnumerator ProcessBattleCamera()
        {
            cameraManager.AssignCamera("Second");

            yield return new WaitForSeconds(1f);

            StartBattle();
        }

        public void DeploySoldier(Soldier soldier)
        {
            if (deployPositions.Count > 0)
            {
                Vector3 deployPos = deployPositions.Dequeue();

                soldier.SetAnimator("Running", true);
                soldier.transform.DOMove(deployPos, deploySpeed)
                    .SetEase(Ease.Linear).SetSpeedBased(true)
                    .OnComplete(() => { soldier.SetAnimator("Running", false); });
                soldier.transform.DOLookAt(new Vector3(deployPos.x, soldier.transform.position.y,
                    deployPos.z), 0.5f);
            }
            else
            {
                Debug.LogError($"Max Capacity:{rowSize * rowCount} reached!!!");
            }

            int startRatio = Mathf.CeilToInt(SoldierManager.Soldiers.Count / 7);

            if (currentIndex == startRatio)
            {
                ToggleBattleCamera();
                currentIndex++;
            }
            else
            {
                currentIndex++;
            }
        }

        void StartBattle()
        {
            StartCoroutine(ProcessBattle());
        }

        IEnumerator ProcessBattle()
        {
            foreach (Enemy enemy in EnemyManager.Enemies)
            {
                enemy.StartAttack();
            }

            yield return new WaitForSeconds(2f);

            foreach (Soldier soldier in SoldierManager.Soldiers)
            {
                soldier.StartShooting();
            }
            enemyManager.StartSpawningEnemies();
        }

        void SetSquareFormation()
        {
            deployPositions.Clear();

            float offsetX = Mathf.Abs((deployPosMin.position.x - deployPosMax.position.x) / rowSize);
            float middleX = deployPosMin.position.x +
                            Mathf.Abs((deployPosMin.position.x - deployPosMax.position.x) / 2);

            Vector3 pos = new Vector3(middleX, deployPosMin.position.y, deployPosMin.position.z);

            float zigzagRowX = 0;
            bool switchDirection = true;
            float toggleOffsetX = 0;

            for (int i = 0; i < rowCount; i++)
            {
                deployPositions.Enqueue(pos);

                //         DEBUG
                // GameObject box1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // box1.GetComponent<BoxCollider>().enabled = false;
                // box1.transform.position = pos;

                for (int j = 0; j < rowSize; j++)
                {
                    //Adjust left and right deploy positions in order
                    if (switchDirection)
                    {
                        pos = new Vector3((pos.x + (toggleOffsetX * 2)) + offsetX, pos.y, pos.z);
                        toggleOffsetX = pos.x - (middleX + zigzagRowX);
                        switchDirection = !switchDirection;
                    }
                    else
                    {
                        pos = new Vector3(pos.x - (toggleOffsetX * 2), pos.y, pos.z);
                        switchDirection = !switchDirection;
                    }

                    deployPositions.Enqueue(pos);

                    //          DEBUG
                    // GameObject box2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    // box2.GetComponent<BoxCollider>().enabled = false;
                    // box2.transform.position = pos;
                }

                //Toggle zigzag formation value between 0 and target value
                zigzagRowX = zigzagRowOffset - zigzagRowX;

                toggleOffsetX = 0;
                pos = new Vector3(middleX + zigzagRowX, pos.y, pos.z - offsetBetweenRows);
            }
        }
    }
}