using UnityEngine;
using UnityEngine.InputSystem;

public class StickInputPresenter : MonoBehaviour
{
    [SerializeField] private InputActionReference _growAction;
    [SerializeField] private StickController _stickController;

    private void OnEnable()
    {
        _growAction.action.started += OnGrowthStarted;
        _growAction.action.canceled += OnGrowthCanceled;

        _growAction.action.Enable();
    }

    private void OnDisable()
    {
        _growAction.action.started -= OnGrowthStarted;
        _growAction.action.canceled -= OnGrowthCanceled;

        _growAction.action.Disable();
    }

    private void OnGrowthStarted(InputAction.CallbackContext context)
    {
        _stickController.BeginGrowth();
    }

    private void OnGrowthCanceled(InputAction.CallbackContext context)
    {
        _stickController.EndGrowth();
    }
}
