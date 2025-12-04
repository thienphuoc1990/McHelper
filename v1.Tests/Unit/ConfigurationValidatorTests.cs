using AutoVPT.Domain;
using AutoVPT.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoVPT.Tests.Unit
{
    [TestClass]
    public class ConfigurationValidatorTests
    {
        private ConfigurationValidator _validator;
        private CharacterAggregate _character;

        [TestInitialize]
        public void Setup()
        {
            _validator = new ConfigurationValidator();
            _character = new CharacterAggregate("test-char", "http://game.url");
        }

        #region ValidateCharacterIdentity Tests

        [TestMethod]
        public void ValidateCharacterIdentity_WithValidCharacter_ReturnsValid()
        {
            // Act
            var result = _validator.ValidateCharacterIdentity(_character);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateCharacterIdentity_WithNullCharacter_ReturnsInvalid()
        {
            // Act
            var result = _validator.ValidateCharacterIdentity(null);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("null")));
        }

        [TestMethod]
        public void ValidateCharacterIdentity_WithEmptyId_ReturnsInvalid()
        {
            // Arrange
            _character.Identity.Id = "";

            // Act
            var result = _validator.ValidateCharacterIdentity(_character);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("ID")));
        }

        [TestMethod]
        public void ValidateCharacterIdentity_WithEmptyLink_ReturnsWarning()
        {
            // Arrange
            _character.Identity.Link = "";

            // Act
            var result = _validator.ValidateCharacterIdentity(_character);

            // Assert
            Assert.IsTrue(result.IsValid); // Still valid, just warning
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("link")));
        }

        #endregion

        #region ValidateAutoPhuBan Tests

        [TestMethod]
        public void ValidateAutoPhuBan_WithValidDungeons_ReturnsValid()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            _character.FeatureConfig.SetParameter(FeatureType.AutoPhuBan, "DanhSach", "Liệt Diễm Thâm Uyên,Trở Lại Lang Huyệt");

            // Act
            var result = _validator.ValidateFeature(FeatureType.AutoPhuBan, _character);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateAutoPhuBan_WithEmptyDungeonList_ReturnsInvalid()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            _character.FeatureConfig.SetParameter(FeatureType.AutoPhuBan, "DanhSach", "");

            // Act
            var result = _validator.ValidateFeature(FeatureType.AutoPhuBan, _character);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("empty")));
        }

        [TestMethod]
        public void ValidateAutoPhuBan_WithUnknownDungeon_ReturnsWarning()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            _character.FeatureConfig.SetParameter(FeatureType.AutoPhuBan, "DanhSach", "Invalid Dungeon,Liệt Diễm Thâm Uyên");

            // Act
            var result = _validator.ValidateFeature(FeatureType.AutoPhuBan, _character);

            // Assert
            Assert.IsTrue(result.IsValid); // Still valid, just warning
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("Unknown")));
        }

        #endregion

        #region ValidateTrongNL Tests

        [TestMethod]
        public void ValidateTrongNL_WithValidMaterial_ReturnsValid()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.TrongNL);
            _character.FeatureConfig.SetParameter(FeatureType.TrongNL, "LoaiNL", "Thiên Tinh Thảo");

            // Act
            var result = _validator.ValidateFeature(FeatureType.TrongNL, _character);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateTrongNL_WithUnknownMaterial_ReturnsWarning()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.TrongNL);
            _character.FeatureConfig.SetParameter(FeatureType.TrongNL, "LoaiNL", "Unknown Material");

            // Act
            var result = _validator.ValidateFeature(FeatureType.TrongNL, _character);

            // Assert
            Assert.IsTrue(result.IsValid); // Still valid, just warning
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("Unknown")));
        }

        #endregion

        #region ValidateDauPet Tests

        [TestMethod]
        public void ValidateDauPet_WithValidCount_ReturnsValid()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.DauPet);
            _character.FeatureConfig.SetParameter(FeatureType.DauPet, "SoLan", "10");

            // Act
            var result = _validator.ValidateFeature(FeatureType.DauPet, _character);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void ValidateDauPet_WithInvalidCount_ReturnsInvalid()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.DauPet);
            _character.FeatureConfig.SetParameter(FeatureType.DauPet, "SoLan", "abc");

            // Act
            var result = _validator.ValidateFeature(FeatureType.DauPet, _character);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.Errors.Any(e => e.Contains("Invalid")));
        }

        [TestMethod]
        public void ValidateDauPet_WithHighCount_ReturnsWarning()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.DauPet);
            _character.FeatureConfig.SetParameter(FeatureType.DauPet, "SoLan", "150");

            // Act
            var result = _validator.ValidateFeature(FeatureType.DauPet, _character);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(result.Warnings.Any(w => w.Contains("high")));
        }

        #endregion

        #region ValidateAll Tests

        [TestMethod]
        public void ValidateAll_WithMultipleEnabledFeatures_ValidatesAll()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);
            _character.FeatureConfig.Enable(FeatureType.TuHanh);

            // Act
            var results = _validator.ValidateAll(_character);

            // Assert
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.ContainsKey(FeatureType.VipPromotion));
            Assert.IsTrue(results.ContainsKey(FeatureType.DoiNangNo));
            Assert.IsTrue(results.ContainsKey(FeatureType.TuHanh));
        }

        [TestMethod]
        public void GetValidationSummary_WithMixedResults_ReturnsCorrectSummary()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            // AutoPhuBan has no DanhSach, so it will be invalid

            // Act
            var results = _validator.ValidateAll(_character);
            var summary = ConfigurationValidator.GetValidationSummary(results);

            // Assert
            Assert.IsTrue(summary.Contains("valid"));
            Assert.IsTrue(summary.Contains("invalid"));
        }

        #endregion
    }
}

