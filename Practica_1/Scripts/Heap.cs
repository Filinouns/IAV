namespace Game_Tank { 

    using System;

    public class Heap<T> where T : IHeapItem<T> {

        T[] items_;
        int currentItemCount;

        public Heap(int maxHeapSize) {
            items_ = new T[maxHeapSize];
        }

        //Añade una casilla al arbol
        public void Add(T item) {
            item.HeapIndex = currentItemCount;
            items_[currentItemCount] = item;
            SortUp(item);
            currentItemCount++;
        }

        // Elimina la casilla principal de todas (maximo padre)
        public T RemoveFirst() {
            T firstItem = items_[0];
            currentItemCount--;
            items_[0] = items_[currentItemCount];
            items_[0].HeapIndex = 0;
            SortDown(items_[0]);
            return firstItem;
        }

        // Actualiza una casilla del arbol
        public void UpdateItem(T item) {
            SortUp(item);
        }

        // Indica la cuenta de items
        public int Count {
            get {
                return currentItemCount;
            }
        }

        //Confirma que el arbol contiene una casilla
        public bool Contains(T item) {
            return Equals(items_[item.HeapIndex], item);
        }

        // Va comparando los padres con los hijos hasta encontrar su lugar
        void SortDown(T item) {
            while (true) {
                int childIndexL = item.HeapIndex * 2 + 1;
                int childIndexR = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childIndexL < currentItemCount) {
                    swapIndex = childIndexL;
                    if (childIndexR < currentItemCount) {
                        if (items_[childIndexL].CompareTo(items_[childIndexR]) < 0) {
                            swapIndex = childIndexR;
                        }
                    }

                    if (item.CompareTo(items_[swapIndex]) < 0) {
                        Swap(item, items_[swapIndex]);
                    }
                    else {
                        return;
                    }

                }
                else {
                    return;
                }
            }
        }

        // Va comparando las casillas "hijos" con los "padres" hasta que encuentra su posicion
        void SortUp(T item) {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true) {
                T parentItem = items_[parentIndex];
                if (item.CompareTo(parentItem) > 0) {
                    Swap(item, parentItem);
                }
                else {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        void Swap(T itemA, T itemB) {
            items_[itemA.HeapIndex] = itemB;
            items_[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }

    }

    public interface IHeapItem<T> : IComparable<T> {
        int HeapIndex {
            get;
            set;
        }
    }

}