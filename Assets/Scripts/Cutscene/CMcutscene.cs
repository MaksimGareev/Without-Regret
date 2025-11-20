using UnityEngine;
using System.Collections;
using Cinemachine;

namespace Ploopploop.CMBasic
{
    public class CMCutscene : MonoBehaviour
    {
        public CinemachineVirtualCamera virtualCamera;
        public CutsceneTrigger cutsceneTrigger;
        private CinemachineTrackedDolly dollyCart;

        public float pathLength;
        public float speed = 5f;
        public float waitTime = 2f;
        private float currentDistance;

        public bool cutScene;

        private void Start()
        {
            dollyCart = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        }

        private void Update()
        {
            if (cutScene && currentDistance <= pathLength)
            {
                currentDistance += speed * Time.deltaTime;
                dollyCart.m_PathPosition = currentDistance;
            }
            else if (cutScene && currentDistance >= pathLength)
            {
                cutScene = false;
                StartCoroutine(EndCutscene());
            }
        }

        public void StartCutscene()
        {
            currentDistance = 0f;
            cutScene = true;
        }

        private IEnumerator EndCutscene()
        {
            yield return new WaitForSeconds(waitTime);
            cutsceneTrigger.EndCutscene();
        }
    }
}
