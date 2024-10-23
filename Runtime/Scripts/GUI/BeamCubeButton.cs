using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeamXR.Streaming.Gui
{
    [RequireComponent(typeof(Button))]
    public class BeamCubeButton : MonoBehaviour
    {
        private GameObject _cube;
        private Button _button;
        private Rigidbody _rigidBody;

        public Collider GetCollider()
        {
            return _cube.GetComponent<Collider>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Create a cube and add it as a child.
            _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Set the cube as a child of the button.
            _cube.transform.SetParent(transform);

            // Get the button component.
            _button = GetComponent<Button>();

            // Give the cube a body.
            _rigidBody = _cube.AddComponent<Rigidbody>();
            _rigidBody.isKinematic = true;

            // Remove the renderer.
            Destroy(_cube.GetComponent<Renderer>());
        }

        // Update is called once per frame
        void Update()
        {
            // Make sure the cube is the same size of the button and is centered.
            _cube.transform.localScale = new Vector3(_button.image.rectTransform.rect.width, _button.image.rectTransform.rect.height, 10);

            // Set the cube position to zero.
            _cube.transform.localPosition = Vector3.zero;

            // Set the cube rotation to zero.
            _cube.transform.localRotation = Quaternion.identity;
        }
    }
}