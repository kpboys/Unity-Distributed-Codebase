using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static FlowerLight.FlowerLightLogic;

[RequireComponent(typeof(Collider))]
public class FlowerLight : MonoBehaviour
{
    public XRGrabInteractable interactable;
    public Color newLampColor;
    public Material lampLightMat;
    public GameObject lamp;

    private Color originalLightColor;
    private LightWrapper[] lampLights;

    private FlowerLightLogic logic;

    // Start is called before the first frame update
    void Start()
    {
        originalLightColor = lampLightMat.color;
        MaterialWrapper materialWrapper = new MaterialWrapper(lampLightMat);
        Light[] lights = lamp.GetComponentsInChildren<Light>();
        lampLights = new LightWrapper[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            lampLights[i] = new LightWrapper(lights[i]);
        }
        logic = new FlowerLightLogic(newLampColor, originalLightColor, materialWrapper, lampLights);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == interactable.gameObject)
            logic.TriggerEnter();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == interactable.gameObject)
            logic.TriggerExit();
    }

    public class FlowerLightLogic
    {
        public interface ILight { Color color { get; set; } }
        public interface IMaterial { Color color { get; set; } }
        public class MaterialWrapper : IMaterial
        {
            private Material material;
            public MaterialWrapper(Material material)
            {
                this.material = material;
            }
            public Color color
            {
                get => material.GetColor("_EmissionColor");
                set => material.SetColor("_EmissionColor", value);
            }
        }
        public class LightWrapper : ILight
        {
            private Light light;
            public LightWrapper(Light light)
            {
                this.light = light;
            }
            public Color color
            {
                get => light.color;
                set => light.color = value;
            }
        }

        private Color newColor;
        private Color originalColor;

        private IMaterial lampLightMat;
        private ILight[] lampLights;

        public FlowerLightLogic(Color newColor, Color originalColor, IMaterial lampLightMat, ILight[] lampLights)
        {
            this.newColor = newColor;
            this.originalColor = originalColor;
            this.lampLightMat = lampLightMat;
            this.lampLights = lampLights;
        }

        public void TriggerEnter()
        {
            ChangeLight(newColor);
        }

        public void TriggerExit()
        {
            ChangeLight(originalColor);
        }

        private void ChangeLight(Color color)
        {
            lampLightMat.color = color;
            foreach (ILight light in lampLights)
            {
                light.color = color;
            }
        }

    }

}
