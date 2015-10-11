using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class TransferRegion : MonoBehaviour {
    [SerializeField]
    private TransferRegion pair;
    private Vector3 scale;
    public bool scaleManuallyAltered {
        get; private set;
    }

    // Use this for initialization
    void Start () {
        scale = gameObject.transform.localScale;
        scaleManuallyAltered = false;
    }

    // Update is called once per frame
    void Update () {
#if UNITY_EDITOR
        if (scale != gameObject.transform.localScale) {
            scaleManuallyAltered = true;
            scale = gameObject.transform.localScale;
        }
        if (pair && scaleManuallyAltered && !pair.scaleManuallyAltered) {
            pair.transform.localScale = scale;
        }
        scaleManuallyAltered = false;
#endif
    }
}
