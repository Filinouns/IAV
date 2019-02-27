
using UnityEngine;

namespace Game_Tank { 
    public class Tank : Casilla {

        public float movementSpeed = 1f;

        private Vector3 targetPosition;
        private Vector3 towardsTarget;

        private bool haveObjetive = false;
        public void setObjetive(bool b) { haveObjetive = b; }

        private int steps = 0;

        public int getSteps() { return steps; }

        //-------------Start---------------
        void Start() {
            
        }

        //----------Update---------------
        void Update() {
            if (haveObjetive) {
                Move();
            }
        }

        public void RecalculateTargetPosition(Vector3 p) {
            targetPosition = p;
            targetPosition.y = 1;
        }

        private void Move() {
            towardsTarget = targetPosition - transform.position;
            if (towardsTarget.magnitude < 0.25f) { //Si llega al destino obtener la siguiente posicion
                steps++;
                RecalculateTargetPosition(board_.MoveTank(steps));
            }

            transform.position += towardsTarget.normalized * movementSpeed * Time.deltaTime;
        }
    }
}