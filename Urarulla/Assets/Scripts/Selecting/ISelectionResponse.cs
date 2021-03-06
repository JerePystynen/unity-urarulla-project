using UnityEngine;

namespace DiMe.Urarulla
{
    public interface ISelectionResponse
    {
        public void OnSelect(Transform selection);
        public void OnDeselect(Transform selection);
        public void SetDotProduct(float distance);
    }
}