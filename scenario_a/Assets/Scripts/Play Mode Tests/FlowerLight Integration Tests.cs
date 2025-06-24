using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.XR.Interaction.Toolkit;

[Category("Integration")]
public class FlowerLightIntegrationTests
{
    private Color newColor = Color.red;
    private Color originalColor = Color.yellow;
    private const string testMatPath = "Assets/Materials/TestMat.mat";
    private static Material testMat;
    private static GameObject cube;
    private static GameObject lamp;
    private static Light lampLight;
    private static FlowerLight flower;

    public IEnumerator Setup()
    {
        testMat = AssetDatabase.LoadAssetAtPath<Material>(testMatPath);
        testMat.SetColor("_EmissionColor", originalColor);

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = Vector3.zero;
        XRGrabInteractable cubeInteractable = cube.AddComponent<XRGrabInteractable>();
        Collider cubeCollider = cube.GetComponent<Collider>();
        cubeCollider.isTrigger = true;
        Rigidbody cubeRB = cube.GetComponent<Rigidbody>();
        cubeRB.useGravity = false;

        lamp = new GameObject("Lamp");
        lampLight = lamp.AddComponent<Light>();
        lampLight.color = originalColor;

        GameObject flowerGO = new GameObject("FlowerLight");
        flowerGO.transform.position = new Vector3(5, 5, 5);
        SphereCollider collider = flowerGO.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        flower = flowerGO.AddComponent<FlowerLight>();
        flower.newLampColor = newColor;
        flower.lampLightMat = testMat;
        flower.interactable = cubeInteractable;
        flower.lamp = lamp;
        yield return null;
    }

    public IEnumerator Teardown()
    {
        GameObject.DestroyImmediate(cube);
        GameObject.DestroyImmediate(lamp);
        GameObject.DestroyImmediate(flower.gameObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LightChangeOnTriggerEnter()
    {
        yield return Setup();

        //Arrange
        Color expectedColor = newColor;

        //Act
        cube.transform.position = flower.transform.position;
        yield return new WaitForSeconds(1);
        Color actualMatColor = testMat.GetColor("_EmissionColor");
        Color actualLightColor = lampLight.color;

        //Assert
        Assert.AreEqual(expectedColor, actualMatColor, "Material color didn't change on enter.");
        Assert.AreEqual(expectedColor, actualLightColor, "Light color didn't change on enter.");

        yield return Teardown();
    }

    [UnityTest]
    public IEnumerator LightChangeOnTriggerExit()
    {
        yield return Setup();

        //Arrange
        Color expectedColor = originalColor;

        //Act
        cube.transform.position = flower.transform.position;
        yield return new WaitForSeconds(1);
        cube.transform.position += new Vector3(5, 5, 5);
        yield return new WaitForSeconds(1);
        Color actualMatColor = testMat.GetColor("_EmissionColor");
        Color actualLightColor = lampLight.color;

        //Assert
        Assert.AreEqual(expectedColor, actualMatColor, "Material color didn't change on exit.");
        Assert.AreEqual(expectedColor, actualLightColor, "Light color didn't change on exit.");

        yield return Teardown();
    }

    [UnityTest]
    public IEnumerator LightChangeMultiplePass()
    {
        yield return Setup();

        //Arrange
        Color expectedEnterColor = newColor;
        Color expectedExitColor = originalColor;

        const int pass = 5;
        List<Color> actualEnterMatColors = new List<Color>();
        List<Color> actualEnterLightColors = new List<Color>();
        List<Color> actualExitMatColors = new List<Color>();
        List<Color> actualExitLightColors = new List<Color>();

        Vector3 enterPos = flower.transform.position;
        Vector3 exitPos = cube.transform.position;

        //Act
        for (int i = 0; i < pass; i++)
        {
            cube.transform.position = enterPos;
            yield return new WaitForSeconds(1);
            Color actualEnterMatColor = testMat.GetColor("_EmissionColor");
            actualEnterMatColors.Add(actualEnterMatColor);
            Color actualEnterLightColor = lampLight.color;
            actualEnterLightColors.Add(actualEnterLightColor);

            cube.transform.position = exitPos;
            yield return new WaitForSeconds(1);
            Color actualExitMatColor = testMat.GetColor("_EmissionColor");
            actualExitMatColors.Add(actualExitMatColor);
            Color actualExitLightColor = lampLight.color;
            actualExitLightColors.Add(actualExitLightColor);
        }

        //Assert
        for (int i = 0; i < pass; i++)
        {
            Assert.AreEqual(expectedEnterColor, actualEnterMatColors[i], $"Material color didn't change on enter, pass: {i}.");
            Assert.AreEqual(expectedEnterColor, actualEnterLightColors[i], $"Light color didn't change on enter, pass: {i}.");

            Assert.AreEqual(expectedExitColor, actualExitMatColors[i], $"Material color didn't change on exit, pass: {i}.");
            Assert.AreEqual(expectedExitColor, actualExitLightColors[i], $"Light color didn't change on exit, pass: {i}.");
        }

        yield return Teardown();
    }

}
