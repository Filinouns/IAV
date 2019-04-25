
using UnityEngine;

namespace Game_Tank {
    public class Tank : Casilla {

        public float movementSpeed = 1f;

        private Position tPos;
        private Vector3 targetPosition;
        private Vector3 towardsTarget;

        private bool haveObjetive = false;
        public void setObjetive(bool b) { haveObjetive = b; }

        private int steps_ = 0;

        public int getSteps() { return steps_; }
        public void setSteps(int s) { steps_ = s; }

        private int SearchMode_;
        public void setMode(int s) { SearchMode_ = s; }
        public int getMode() { return SearchMode_; }

        public bool selected {
            get;
            set;
        }

        //-------------Start---------------
        void Start() {
        }

        //----------Update---------------
        void Update() {

            if (Input.GetButtonDown("Jump")) {
                RecalculateTargetPosition(board_.MoveTank(steps_));
            }
            if (haveObjetive) {
                Move();
            }
        }

        public void RecalculateTargetPosition(Casilla p) {
            targetPosition = p.transform.position;
            targetPosition.y = 1;
            tPos = p.pos;
        }

        private void Move() {
            towardsTarget = targetPosition - transform.position;

            if (towardsTarget.magnitude < 0.25f) { //Si llega al destino obtener la siguiente posicion
                steps_++;
                Casilla nextObj = board_.MoveTank(steps_);
                if (nextObj != null) RecalculateTargetPosition(nextObj);
            }

            transform.position += towardsTarget.normalized * movementSpeed * Time.deltaTime;
            pos = tPos;
        }

       

        private float mediaDe2(float x, float y) {
            float media = 1;

            media = (x + y) / 2;

            return media;
        }

        private float resta(float x, float y)
        {
            float resta;

            resta = Mathf.Abs(x) - Mathf.Abs(y);
            Mathf.Abs(resta);

            return resta;
        }
    }
}