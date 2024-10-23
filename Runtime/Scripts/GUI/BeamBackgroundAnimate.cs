using UnityEngine;
using UnityEngine.UI;

public class BeamBackgroundAnimate : MonoBehaviour
{
    private Material _material;

    [SerializeField]
    private float _xSpeed = 0.03f;

    [SerializeField]
    private float _ySpeed = 0.01f;

    private Vector2 _offset = Vector2.zero;

    [SerializeField]
    [Range(0f, 360f)]
    private float _rotationSpeed = 30f;  // degrees per second

    [SerializeField]
    [Range(0f, 1f)]
    private float _transparency = 1f;  // Transparency level

    private float _currentRotation = 0f;

    void Update()
    {
        if (_material == null || _material.mainTexture == null)
        {
            var image = GetComponent<Image>();

            if (image != null)
            {
                _material = image.material;
            }
        }

        if (_material == null)
        {
            return;
        }

        // Update the material offset.
        _offset.x += _xSpeed * Time.deltaTime;
        _offset.y += _ySpeed * Time.deltaTime;

        if (_offset.x > 0.6f)
        {
            _offset.x = 0.6f;
            _xSpeed = -_xSpeed;
        }

        if (_offset.y > 0.6f)
        {
            _offset.y = 0.6f;
            _ySpeed = -_ySpeed;
        }

        if (_offset.x < 0.3f)
        {
            _offset.x = 0.3f;
            _xSpeed = -_xSpeed;
        }

        if (_offset.y < 0.3f)
        {
            _offset.y = 0.3f;
            _ySpeed = -_ySpeed;
        }

        // Update the rotation angle
        _currentRotation += _rotationSpeed * Time.deltaTime;
        if (_currentRotation > 360f) _currentRotation -= 360f;
        if (_currentRotation < 0f) _currentRotation += 360f;

        // Apply the rotation, offset, and transparency to the material
        _material.mainTextureOffset = _offset;
        _material.SetFloat("_Rotation", _currentRotation * Mathf.Deg2Rad);
        _material.SetFloat("_Transparency", _transparency);
    }

    // Method to set the rotation speed from the radial dial
    public void SetRotationSpeed(float speed)
    {
        _rotationSpeed = speed;
    }

    // Method to set the transparency from the radial dial
    public void SetTransparency(float transparency)
    {
        _transparency = transparency;
    }
}
