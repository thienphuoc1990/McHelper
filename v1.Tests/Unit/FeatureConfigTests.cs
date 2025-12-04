using AutoVPT.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoVPT.Tests.Unit
{
    [TestClass]
    public class FeatureConfigTests
    {
        #region FeatureConfig Tests

        [TestMethod]
        public void FeatureConfig_Constructor_InitializesDefaults()
        {
            // Act
            var config = new FeatureConfig();

            // Assert
            Assert.IsNotNull(config.Parameters);
            Assert.IsFalse(config.Enabled);
        }

        [TestMethod]
        public void FeatureConfig_GetParameter_ReturnsValue()
        {
            // Arrange
            var config = new FeatureConfig(FeatureType.AutoPhuBan);
            config.SetParameter("DanhSach", "Test Dungeon");

            // Act
            var value = config.GetParameter("DanhSach");

            // Assert
            Assert.AreEqual("Test Dungeon", value);
        }

        [TestMethod]
        public void FeatureConfig_GetParameter_ReturnsDefault()
        {
            // Arrange
            var config = new FeatureConfig(FeatureType.AutoPhuBan);

            // Act
            var value = config.GetParameter("NonExistent", "DefaultValue");

            // Assert
            Assert.AreEqual("DefaultValue", value);
        }

        [TestMethod]
        public void FeatureConfig_HasParameter_ReturnsTrue()
        {
            // Arrange
            var config = new FeatureConfig(FeatureType.AutoPhuBan);
            config.SetParameter("DanhSach", "Test");

            // Act & Assert
            Assert.IsTrue(config.HasParameter("DanhSach"));
        }

        [TestMethod]
        public void FeatureConfig_HasParameter_ReturnsFalse()
        {
            // Arrange
            var config = new FeatureConfig(FeatureType.AutoPhuBan);

            // Act & Assert
            Assert.IsFalse(config.HasParameter("NonExistent"));
        }

        #endregion

        #region CharacterFeatureConfig Tests

        [TestMethod]
        public void CharacterFeatureConfig_Constructor_InitializesAllFeatures()
        {
            // Act
            var config = new CharacterFeatureConfig("test-char");

            // Assert
            Assert.AreEqual("test-char", config.CharacterId);
            Assert.IsNotNull(config.Features);
            Assert.IsTrue(config.Features.Count > 0);
        }

        [TestMethod]
        public void CharacterFeatureConfig_Enable_EnablesFeature()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");

            // Act
            config.Enable(FeatureType.VipPromotion);

            // Assert
            Assert.IsTrue(config.IsEnabled(FeatureType.VipPromotion));
        }

        [TestMethod]
        public void CharacterFeatureConfig_Disable_DisablesFeature()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");
            config.Enable(FeatureType.VipPromotion);

            // Act
            config.Disable(FeatureType.VipPromotion);

            // Assert
            Assert.IsFalse(config.IsEnabled(FeatureType.VipPromotion));
        }

        [TestMethod]
        public void CharacterFeatureConfig_Enable_WithParameters_SetsParameters()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");
            var parameters = new System.Collections.Generic.Dictionary<string, string>
            {
                { "DanhSach", "Dungeon1,Dungeon2" }
            };

            // Act
            config.Enable(FeatureType.AutoPhuBan, parameters);

            // Assert
            Assert.IsTrue(config.IsEnabled(FeatureType.AutoPhuBan));
            Assert.AreEqual("Dungeon1,Dungeon2", config.GetParameter(FeatureType.AutoPhuBan, "DanhSach"));
        }

        [TestMethod]
        public void CharacterFeatureConfig_GetConfig_ReturnsFeatureConfig()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");
            config.Enable(FeatureType.VipPromotion);

            // Act
            var featureConfig = config.GetConfig(FeatureType.VipPromotion);

            // Assert
            Assert.IsNotNull(featureConfig);
            Assert.AreEqual(FeatureType.VipPromotion, featureConfig.Type);
            Assert.IsTrue(featureConfig.Enabled);
        }

        [TestMethod]
        public void CharacterFeatureConfig_GetParameter_ReturnsValue()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");
            config.SetParameter(FeatureType.AutoPhuBan, "DanhSach", "Test Value");

            // Act
            var value = config.GetParameter(FeatureType.AutoPhuBan, "DanhSach");

            // Assert
            Assert.AreEqual("Test Value", value);
        }

        [TestMethod]
        public void CharacterFeatureConfig_GetParameter_ReturnsDefault()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");

            // Act
            var value = config.GetParameter(FeatureType.AutoPhuBan, "NonExistent", "Default");

            // Assert
            Assert.AreEqual("Default", value);
        }

        [TestMethod]
        public void CharacterFeatureConfig_SetParameter_CreatesFeatureIfNotExists()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");

            // Act
            config.SetParameter(FeatureType.AutoPhuBan, "Key", "Value");

            // Assert
            Assert.IsTrue(config.Features.ContainsKey(FeatureType.AutoPhuBan));
            Assert.AreEqual("Value", config.GetParameter(FeatureType.AutoPhuBan, "Key"));
        }

        [TestMethod]
        public void CharacterFeatureConfig_IsEnabled_ReturnsFalseByDefault()
        {
            // Arrange
            var config = new CharacterFeatureConfig("test-char");

            // Assert
            Assert.IsFalse(config.IsEnabled(FeatureType.VipPromotion));
        }

        #endregion
    }
}

