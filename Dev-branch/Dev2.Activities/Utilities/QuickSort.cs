using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unlimited.Applications.BusinessDesignStudio.Activities.Utilities {
    public class QuickSort<T> where T : IComparable<T> {
        // the sort threshold to determine parallel processing
        const int Threshold = 4096;
        private string[] _arrayToDataListObjectMap;
        private bool _sortDescending;
        private bool _sortObjectOnly = false; 

        public string[] SortedDatalist {
            get {
                return _arrayToDataListObjectMap;
            }
        }


        public string[] StartSort(T[] toSort, int left, int right, IEnumerable<string> dataList, bool desc) {
            _arrayToDataListObjectMap = dataList.ToArray();
            _sortDescending = desc;
            Sort(toSort, left, right, Environment.ProcessorCount);
            return _arrayToDataListObjectMap;
        }

        public T[] StartSort(T[] toSort, int left, int right, bool desc) {
            Sort(toSort, left, right, Environment.ProcessorCount);
            _sortObjectOnly = true;
            return toSort;
        }

        public void Sort(T[] toSort, int left, int right, int maxDepth) {
            if (right > left) {
                if (maxDepth < 1 || (right - left) < Threshold) {
                    SequentialSort(toSort, left, right);
                }
                else {
                    --maxDepth;
                    int pivot = Partition(toSort, left, right);
                    Parallel.Invoke(
                        () => Sort(toSort, left, pivot - 1, maxDepth),
                        () => Sort(toSort, pivot + 1, right, maxDepth));
                }
            }
        }

        private void SequentialSort(T[] toSort, int left, int right) {
            if (right <= left) return;
            int pivot = Partition(toSort, left, right);
            SequentialSort(toSort, left, pivot - 1);
            SequentialSort(toSort, pivot + 1, right);
        }

        private void Swap(T[] arr, int i, int j) {
            if (!_sortObjectOnly) {
                // sort the the recordSet with regards to the sort array
                string tmp = _arrayToDataListObjectMap[i];
                _arrayToDataListObjectMap[i] = _arrayToDataListObjectMap[j];
                _arrayToDataListObjectMap[j] = tmp;
            }
            T sortertmp = arr[i];
            arr[i] = arr[j];
            arr[j] = sortertmp;
        }

        private int Partition(T[] arr, int low, int high) {
            // Simply partitioning implementation

            int pivotPos = low + (high - low) / 2;
            T pivot = arr[pivotPos];
            Swap(arr, low, pivotPos);

            int left = low;
            // find anything less than the pivot point
            for (int i = low + 1; i <= high; i++) {
                if (_sortDescending) {
                    if (arr[i].CompareTo(pivot) >= 0) {
                        left++;
                        Swap(arr, i, left);
                    }
                }
                else {
                    if (arr[i].CompareTo(pivot) <= 0) {
                        left++;
                        Swap(arr, i, left);
                    }
                }
            }

            Swap(arr, low, left);

            return left;
        }
    }
}
