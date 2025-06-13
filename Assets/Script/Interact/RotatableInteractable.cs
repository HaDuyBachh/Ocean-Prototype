using UnityEngine;
using UnityEngine.Events;

public class RotatableInteractable : MonoBehaviour
{
    public Transform targetToRotate;
    public Vector3 rotationAxis = Vector3.right;
    public float rotationMultiplier = 1f;
    public float maxRotation = 180f;
    public UnityEvent OnRotationCompleted;
    public UnityEvent OnCancelAction;

    private float totalRotated = 0f;
    private float lastAngle = 0f;
    private bool isRotating = false;
    private bool isCompleted = false;

    public void StartRotate()
    {
        if (isCompleted)
        {
            CancelAction();
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        lastAngle = GetMouseAngle();
        isRotating = true;
    }

    public void CancelAction()
    {
        OnCancelAction.Invoke();
    }

    public void StopRotate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isRotating = false;
    }

    void Update()
    {
        if (!isRotating || isCompleted) return;

        if (Input.GetMouseButton(0))
        {
            float currentAngle = GetMouseAngle();
            float delta = Mathf.DeltaAngle(lastAngle, currentAngle);
            lastAngle = currentAngle;

            float rotateAmount = delta * rotationMultiplier * Time.deltaTime;
            totalRotated += Mathf.Abs(rotateAmount);

            targetToRotate.Rotate(rotationAxis, rotateAmount, Space.Self);

            if (totalRotated >= maxRotation)
            {
                isCompleted = true;
                StopRotate();
                OnRotationCompleted?.Invoke();
            }
        }
        else
        {
            StopRotate();
        }
    }

    float GetMouseAngle()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 dir = ((Vector2)Input.mousePosition - screenCenter).normalized;
        return Vector2.SignedAngle(Vector2.up, dir);
    }
}
