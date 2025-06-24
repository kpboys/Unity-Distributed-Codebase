using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using static FlowerLight;
using static FlowerLight.FlowerLightLogic;

[Category("Unit")]
public class FlowerLightUnitTests
{
    private ILight[] CreateLampLights(Color originalColor)
    {
        ILight[] lampLights = new ILight[4] { SubstituteLight(), SubstituteLight(), SubstituteLight(), SubstituteLight() };
        return lampLights;
        ILight SubstituteLight()
        {
            ILight light = Substitute.For<ILight>();
            light.color = originalColor;
            return light;
        }
    }

    [Test]
    public void TriggerEnterLightChangePass()
    {
        //Arrange
        Color newColor = Color.red;
        Color originalColor = Color.yellow;
        IMaterial lampLightMat = Substitute.For<IMaterial>();
        lampLightMat.color = originalColor;
        ILight[] lampLights = CreateLampLights(originalColor);

        FlowerLightLogic logic = new FlowerLightLogic(newColor, originalColor, lampLightMat, lampLights);

        Color expectedColor = newColor;

        //Act
        logic.TriggerEnter();
        Color actualMatColor = lampLightMat.color;
        Color actualLightColor = lampLights[0].color;

        //Assert
        Assert.AreEqual(expectedColor, actualMatColor, "Material didn't change color on enter.");
        Assert.AreEqual(expectedColor, actualLightColor, "Light didn't change color on enter.");
    }

    [Test]
    public void TriggerExitLightChangePass()
    {
        //Arrange
        Color newColor = Color.red;
        Color originalColor = Color.yellow;
        IMaterial lampLightMat = Substitute.For<IMaterial>();
        lampLightMat.color = originalColor;
        ILight[] lampLights = CreateLampLights(originalColor);

        FlowerLightLogic logic = new FlowerLightLogic(newColor, originalColor, lampLightMat, lampLights);

        Color expectedExitColor = originalColor;

        //Act
        logic.TriggerEnter();
        logic.TriggerExit();
        Color actualExitMatColor = lampLightMat.color;
        Color actualExitLightColor = lampLights[0].color;

        //Assert
        Assert.AreEqual(expectedExitColor, actualExitMatColor, "Material didn't change color on exit.");
        Assert.AreEqual(expectedExitColor,actualExitLightColor, "Light didn't change color on exit.");
    }

    [Test]
    public void FullLightChangePass()
    {
        //Arrange
        Color newColor = Color.red;
        Color originalColor = Color.yellow;
        IMaterial lampLightMat = Substitute.For<IMaterial>();
        lampLightMat.color = originalColor;
        ILight[] lampLights = CreateLampLights(originalColor);

        FlowerLightLogic logic = new FlowerLightLogic(newColor, originalColor, lampLightMat, lampLights);

        Color expectedEnterColor = newColor;
        Color expectedExitColor = originalColor;

        //Act
        logic.TriggerEnter();
        Color actualEnterMatColor = lampLightMat.color;
        Color actualEnterLightColor = lampLights[0].color;

        logic.TriggerExit();
        Color actualExitMatColor = lampLightMat.color;
        Color actualExitLightColor = lampLights[0].color;

        //Assert
        Assert.AreEqual(expectedEnterColor, actualEnterMatColor, "Material didn't change color on enter.");
        Assert.AreEqual(expectedEnterColor, actualEnterLightColor, "Light didn't change color on enter.");

        Assert.AreEqual(expectedExitColor, actualExitMatColor, "Material didn't change color on exit.");
        Assert.AreEqual(expectedExitColor, actualExitLightColor, "Light didn't change color on exit.");
    }

}
