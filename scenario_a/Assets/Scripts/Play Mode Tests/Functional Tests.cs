using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

[Category("Functional")]
public class FunctionalTests
{

    private const string simulatorPath = "Assets/Samples/XR Interaction Toolkit/2.6.4/XR Device Simulator/XR Device Simulator.prefab";
    private static Keyboard virtualKeyboard;
    private static Mouse virtualMouse;
    private static GameObject simulatorInstance;

    private const string playerObjectName = "XR Interaction Setup";
    private GameObject player { get { return GameObject.Find(playerObjectName); } }
    private const string leftFlowerLightName = "Left Treestump";
    private const string rightFlowerLightName = "Right Treestump";
    private FlowerLight leftFlowerLight { get { return GameObject.Find(leftFlowerLightName).GetComponent<FlowerLight>(); } }
    private FlowerLight rightFlowerLight { get { return GameObject.Find(rightFlowerLightName).GetComponent<FlowerLight>(); } }
    private const string lampName = "GlowLamp";
    private GameObject lamp { get { GameObject _lamp = GameObject.Find(lampName); Assert.IsNotNull(_lamp, "Lamp not found."); return _lamp; } }
    private Light[] lampLights { get { return lamp.GetComponentsInChildren<Light>(); } }

    private static Color lampMatOriginalColor;

    public IEnumerator Setup()
    {
        SetDevices(false);
        virtualKeyboard = InputSystem.AddDevice<Keyboard>("Virtual Keyboard");
        virtualMouse = InputSystem.AddDevice<Mouse>("Virtual Mouse");

        SceneManager.LoadScene("Main");
        yield return null;

        GameObject simulatorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(simulatorPath);
        simulatorInstance = (GameObject)PrefabUtility.InstantiatePrefab(simulatorPrefab);
        Assert.IsNotNull(simulatorInstance, "Simulator is null.");

        lampMatOriginalColor = leftFlowerLight.lampLightMat.GetColor("_EmissionColor");

        yield return null;
    }

    public IEnumerator Teardown()
    {
        InputSystem.RemoveDevice(virtualKeyboard);
        InputSystem.RemoveDevice(virtualMouse);
        SetDevices(true);

        GameObject.DestroyImmediate(simulatorInstance);
        simulatorInstance = null;

        leftFlowerLight.lampLightMat.SetColor("_EmissionColor", lampMatOriginalColor);

        yield return null;
    }

    private void SetDevices(bool active)
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (active)
                InputSystem.EnableDevice(device);
            else
                InputSystem.DisableDevice(device);
        }
    }

    #region Testing Utilities
    private void PressKey(Key key)
    {
        InputSystem.QueueStateEvent(virtualKeyboard, new KeyboardState(key));
        InputSystem.Update();
    }

    private void RotatePlayer(GameObject player, float rotation)
    {
        player.transform.rotation = Quaternion.Euler(player.transform.eulerAngles + new Vector3(0, rotation, 0));
    }

    private IEnumerator GrabFlower(GameObject player)
    {
        RotatePlayer(player, 7);//Rotate player 7 degrees to the right
        yield return null;
        PressKey(Key.Tab);//Press tab, to controll left controller
        yield return null;
        PressKey(Key.G);//Press G, to grab flower
        yield return null;
        PressKey(Key.U);//Press U, to control player movement
        yield return null;
        RotatePlayer(player, -7);//Rotate player 7 degrees to the left
        yield return null;
    }

    private IEnumerator MoveHorizontal(Keyboard keyboard, bool leftRight, float duration)
    {
        Key key = leftRight ? Key.A : Key.D;
        PressKey(key);
        yield return duration;
        PressKey(Key.None);
        yield return null;
    }

    private IEnumerator WaitForColorChange(Material mat, Light light, float maxDuration = 2)
    {
        float time = 0;
        Color matStart = mat.GetColor("_EmissionColor");
        Color lightStart = light.color;
        while (true)
        {
            Color matCurrent = mat.GetColor("_EmissionColor");
            Color lightCurrent = light.color;
            time += Time.deltaTime;
            if (matCurrent != matStart && lightCurrent != lightStart || time >= maxDuration)
                break;
            yield return null;
        }
    }

    private void AssertColor(Color expected, Color actual) { AssertColor(expected, actual, null); }
    private void AssertColor(Color expected, Color actual, string message)
    {
        ColorEqualityComparer comparor = new ColorEqualityComparer(0.05f);
        bool areEqual = comparor.Equals(expected, actual);
        string errorData = $"\nExpected:{expected} \nActual:{actual}.";
        if (message != null)
            Assert.IsTrue(areEqual, message + errorData);
        else
            Assert.IsTrue(areEqual);
    }
    #endregion

    [UnityTest]
    public IEnumerator FlowerEnterAndExitLeftFlowerLightPass()
    {
        yield return Setup();

        //Arrange
        GameObject player = this.player;
        Assert.IsNotNull(player, "Player not found.");
        FlowerLight leftFlowerLight = this.leftFlowerLight;
        Assert.IsNotNull(leftFlowerLight, "Left flower light not found.");

        Material lampMat = leftFlowerLight.lampLightMat;
        Assert.IsNotNull(lampMat, "Lamp material not found.");

        Light[] lights = lampLights;
        Assert.IsNotNull(lights, "Lamp light components not found.");

        Color expectedStartColor = Color.yellow;
        Color expectedTriggerColor = leftFlowerLight.newLampColor;

        Color actualStartMatColor = lampMat.GetColor("_EmissionColor");
        Color actualStartLightColor = lights[0].color;

        GameObject playerCamera = Camera.main.gameObject;
        Vector3 playerStartPos = playerCamera.transform.position;

        //Act
        yield return GrabFlower(player);//Grab the flower
        PressKey(Key.A);//Walk left
        yield return WaitForColorChange(lampMat, lights[0], 3);
        Color actualTriggerMatColor = lampMat.GetColor("_EmissionColor");
        Color actualTriggerLightColor = lights[0].color;

        Vector3 playerNewPos = playerCamera.transform.position;

        PressKey(Key.D);//Walk right
        yield return WaitForColorChange(lampMat, lights[0], 3);
        Color actualExitMatColor = lampMat.GetColor("_EmissionColor");
        Color actualExitLightColor = lights[0].color;

        //Assert
        Assert.AreNotEqual(playerStartPos, playerNewPos, "Player hasn't moved.");

        AssertColor(expectedStartColor, actualStartMatColor, "Material start color was not as expected.");
        AssertColor(expectedStartColor, actualStartLightColor, "Light start color was not as expected.");

        AssertColor(expectedTriggerColor, actualTriggerMatColor, "Material trigger color was not as expected.");
        AssertColor(expectedTriggerColor, actualTriggerLightColor, "Light trigger color was not as expected.");

        AssertColor(expectedStartColor, actualExitMatColor, "Material exit color was not as expected.");
        AssertColor(expectedStartColor, actualExitLightColor, "Light exit color was not as expected.");

        yield return Teardown();
    }

    [UnityTest]
    public IEnumerator FlowerEnterAndExitRightFlowerLightPass()
    {
        yield return Setup();

        //Arrange
        GameObject player = this.player;
        Assert.IsNotNull(player, "Player not found.");
        FlowerLight rightFlowerLight = this.rightFlowerLight;
        Assert.IsNotNull(rightFlowerLight, "Right flower light not found.");

        Material lampMat = rightFlowerLight.lampLightMat;
        Assert.IsNotNull(lampMat, "Lamp material not found.");

        Light[] lights = lampLights;
        Assert.IsNotNull(lights, "Lamp light components not found.");

        Color expectedStartColor = Color.yellow;
        Color expectedTriggerColor = rightFlowerLight.newLampColor;

        Color actualStartMatColor = lampMat.GetColor("_EmissionColor");
        Color actualStartLightColor = lights[0].color;

        GameObject playerCamera = Camera.main.gameObject;
        Vector3 playerStartPos = playerCamera.transform.position;

        //Act
        yield return GrabFlower(player);//Grab the flower
        PressKey(Key.D);//Walk right
        yield return WaitForColorChange(lampMat, lights[0], 3);
        Color actualTriggerMatColor = lampMat.GetColor("_EmissionColor");
        Color actualTriggerLightColor = lights[0].color;

        Vector3 playerNewPos = playerCamera.transform.position;

        PressKey(Key.A);//Walk left
        yield return WaitForColorChange(lampMat, lights[0], 3);
        Color actualExitMatColor = lampMat.GetColor("_EmissionColor");
        Color actualExitLightColor = lights[0].color;

        //Assert
        Assert.AreNotEqual(playerStartPos, playerNewPos, "Player hasn't moved.");

        AssertColor(expectedStartColor, actualStartMatColor, "Material start color was not as expected.");
        AssertColor(expectedStartColor, actualStartLightColor, "Light start color was not as expected.");

        AssertColor(expectedTriggerColor, actualTriggerMatColor, "Material trigger color was not as expected.");
        AssertColor(expectedTriggerColor, actualTriggerLightColor, "Light trigger color was not as expected.");

        AssertColor(expectedStartColor, actualExitMatColor, "Material exit color was not as expected.");
        AssertColor(expectedStartColor, actualExitLightColor, "Light exit color was not as expected.");

        yield return Teardown();
    }

    [UnityTest]
    public IEnumerator FlowerMultipleColorChangePass()
    {
        yield return Setup();

        //Arrange
        GameObject player = this.player;
        Assert.IsNotNull(player, "Player not found.");
        FlowerLight leftFlowerLight = this.leftFlowerLight;
        Assert.IsNotNull(leftFlowerLight, "Left flower light not found.");
        FlowerLight rightFlowerLight = this.rightFlowerLight;
        Assert.IsNotNull(rightFlowerLight, "Right flower light not found.");

        Material lampMat = rightFlowerLight.lampLightMat;
        Assert.IsNotNull(lampMat, "Lamp material not found.");

        Light[] lights = lampLights;
        Assert.IsNotNull(lights, "Lamp light components not found.");

        Color expectedStartColor = Color.yellow;
        Color expectedTriggerColorRight = rightFlowerLight.newLampColor;
        Color expectedTriggerColorLeft = leftFlowerLight.newLampColor;

        List<Color> actualDefaultMatColors = new List<Color>();
        List<Color> actualDefaultLightColors = new List<Color>();
        List<Color> actualTriggerLeftMatColors = new List<Color>();
        List<Color> actualTriggerLeftLightColors = new List<Color>();
        List<Color> actualTriggerRightMatColors = new List<Color>();
        List<Color> actualTriggerRightLightColors = new List<Color>();

        actualDefaultMatColors.Add(lampMat.GetColor("_EmissionColor"));
        actualDefaultLightColors.Add(lights[0].color);

        int pass = 5;

        GameObject playerCamera = Camera.main.gameObject;
        Vector3 playerStartPos = playerCamera.transform.position;
        Vector3 playerNewPos = playerCamera.transform.position;

        //Act
        yield return GrabFlower(player);//Grab the flower
        for (int i = 0; i < pass; i++)
        {
            PressKey(Key.A);//Walk left
            yield return WaitForColorChange(lampMat, lights[0], 3);//Until light turns green
            actualTriggerLeftMatColors.Add(lampMat.GetColor("_EmissionColor"));
            actualTriggerLeftLightColors.Add(lights[0].color);

            playerNewPos = playerCamera.transform.position;

            PressKey(Key.D);//Walk right
            yield return WaitForColorChange(lampMat, lights[0], 3);//Until light turns yellow
            actualDefaultMatColors.Add(lampMat.GetColor("_EmissionColor"));
            actualDefaultLightColors.Add(lights[0].color);

            //Continue right
            yield return WaitForColorChange(lampMat, lights[0], 3);//Until light turns red
            actualTriggerRightMatColors.Add(lampMat.GetColor("_EmissionColor"));
            actualTriggerRightLightColors.Add(lights[0].color);

            PressKey(Key.A);//Walk left
            yield return WaitForColorChange(lampMat, lights[0], 3);//Until light turns yellow
            actualDefaultMatColors.Add(lampMat.GetColor("_EmissionColor"));
            actualDefaultLightColors.Add(lights[0].color);
        }

        //Assert
        Assert.AreNotEqual(playerStartPos, playerNewPos, "Player hasn't moved.");

        AssertListOfColors(expectedStartColor, actualDefaultMatColors, "Material start color was not as expected.");
        AssertListOfColors(expectedStartColor, actualDefaultLightColors, "Light start color was not as expected.");

        AssertListOfColors(expectedTriggerColorLeft, actualTriggerLeftMatColors, "Left material trigger color was not as expected.");
        AssertListOfColors(expectedTriggerColorLeft, actualTriggerLeftLightColors, "Left light trigger color was not as expected.");

        AssertListOfColors(expectedTriggerColorRight, actualTriggerRightMatColors, "Right material trigger color was not as expected.");
        AssertListOfColors(expectedTriggerColorRight, actualTriggerRightLightColors, "Right light trigger color was not as expected.");

        yield return Teardown();

        void AssertListOfColors(Color expected, List<Color> colors, string message)
        {
            foreach (Color col in colors)
                AssertColor(expected, col, message);
        }
    }

}
