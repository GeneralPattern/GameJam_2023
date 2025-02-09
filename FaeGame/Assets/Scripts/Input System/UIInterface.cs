using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInterface : MonoBehaviour
{
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
    
    public UnityAction<ClickData> SendClickDataToTower;
    public GameInputsSO controls;
    private ClickData _clickData;
    private Camera _cameraMain;
    private CameraUtility _cameraUtility;
    private Vector2 _clickPosition;
    private Vector3 _mouseWorldPosition;
    private bool _isHolding;

    public GameObject prefabObj;
    private GameObject _clone;
    private MeshBehavior _cloneMeshBehavior;
    
    private void Awake()
    {
        _clickData = ScriptableObject.CreateInstance<ClickData>();
        _cameraUtility = ScriptableObject.CreateInstance<CameraUtility>();
        _cameraMain = Camera.main;
        controls.GameInputsObj.DefaultControls.PrimaryPress.performed += OnClick;
        controls.GameInputsObj.DefaultControls.PrimaryPress.canceled += OffClick;
        controls.GameInputsObj.DefaultControls.PrimaryPosition.performed += context => _clickPosition = context.ReadValue<Vector2>();
    }
    
    private void OnEnable() => controls.GameInputsObj.DefaultControls.Enable();
    
    private void OnDisable() => controls.GameInputsObj.DefaultControls.Disable();

    private void OnClick(InputAction.CallbackContext context)
    {
        GetHitPointPosition();
        _isHolding = true;
        _clickData.positionStart = _clickPosition;
        _clone = Instantiate(prefabObj, GetHitPointPosition(), prefabObj.transform.rotation);
        _clone.GetComponentInChildren<Collider>().enabled = false;
        _cloneMeshBehavior = _clone.GetComponentInChildren<MeshBehavior>();
        StartCoroutine(UpdateMousePosition());
    }
    
    private IEnumerator UpdateMousePosition()
    {
        while (_isHolding)
        {
            _clone.transform.position = GetHitPointPosition() + new Vector3(0, _clone.transform.localScale.y, 0);
            yield return _waitForFixedUpdate;
        }
    }
    
    private void OffClick(InputAction.CallbackContext context)
    {
        _isHolding = false;
        _clickData.positionEnd = _clickData.positionCurrent;
        Collider hitObj = GetHitObj();
        if (hitObj != null && hitObj.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            GroundBehavior groundBehavior = hitObj.GetComponent<GroundBehavior>();
            Vector2Int gridLocation = groundBehavior.GetGridLocation();

            _clickData.gridLocation = gridLocation;
            SendClickDataToTower(_clickData);
        }

        Destroy(_clone);
    }
    
    private Vector3 GetHitPointPosition()
    {
        var hit = _cameraUtility.PointToRay(_cameraMain, _clickPosition);
        return hit.point;
    }

    private Collider GetHitObj()
    {
        var hit = _cameraUtility.PointToRay(_cameraMain, _clickPosition);
        return hit.collider;
    }
}
