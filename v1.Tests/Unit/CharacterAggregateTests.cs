using AutoVPT.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace AutoVPT.Tests.Unit
{
    [TestClass]
    public class CharacterAggregateTests
    {
        private CharacterAggregate _character;

        [TestInitialize]
        public void Setup()
        {
            _character = new CharacterAggregate("test-char", "http://game.url");
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithIdAndLink_SetsProperties()
        {
            // Assert
            Assert.AreEqual("test-char", _character.Id);
            Assert.AreEqual("test-char", _character.Identity.Id);
            Assert.AreEqual("http://game.url", _character.Identity.Link);
        }

        [TestMethod]
        public void Constructor_Default_InitializesComponents()
        {
            // Arrange
            var character = new CharacterAggregate();

            // Assert
            Assert.IsNotNull(character.Identity);
            Assert.IsNotNull(character.FeatureConfig);
            Assert.IsNotNull(character.RuntimeState);
        }

        #endregion

        #region ShouldRunFeature Tests

        [TestMethod]
        public void ShouldRunFeature_WhenEnabledAndNotCompleted_ReturnsTrue()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.RuntimeState.Start(); // Sets Mode to Normal

            // Act
            var shouldRun = _character.ShouldRunFeature(FeatureType.VipPromotion);

            // Assert
            Assert.IsTrue(shouldRun);
        }

        [TestMethod]
        public void ShouldRunFeature_WhenDisabled_ReturnsFalse()
        {
            // Arrange
            _character.FeatureConfig.Disable(FeatureType.VipPromotion);
            _character.RuntimeState.Start(); // Sets Mode to Normal

            // Act
            var shouldRun = _character.ShouldRunFeature(FeatureType.VipPromotion);

            // Assert
            Assert.IsFalse(shouldRun);
        }

        [TestMethod]
        public void ShouldRunFeature_WhenCompleted_ReturnsFalse()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.RuntimeState.Start(); // Sets Mode to Normal
            _character.RuntimeState.MarkCompleted(FeatureType.VipPromotion);

            // Act
            var shouldRun = _character.ShouldRunFeature(FeatureType.VipPromotion);

            // Assert
            Assert.IsFalse(shouldRun);
        }

        [TestMethod]
        public void ShouldRunFeature_WhenNotRunning_ReturnsFalse()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.RuntimeState.Stop(); // Sets Mode to Stopped

            // Act
            var shouldRun = _character.ShouldRunFeature(FeatureType.VipPromotion);

            // Assert
            Assert.IsFalse(shouldRun);
        }

        #endregion

        #region CompleteFeature Tests

        [TestMethod]
        public void CompleteFeature_MarksAsCompleted()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            _character.CompleteFeature(FeatureType.VipPromotion);

            // Assert
            Assert.IsTrue(_character.RuntimeState.IsCompleted(FeatureType.VipPromotion));
        }

        #endregion

        #region GetEnabledFeatures Tests

        [TestMethod]
        public void GetEnabledFeatures_ReturnsOnlyEnabled()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);
            _character.FeatureConfig.Disable(FeatureType.TuHanh);

            // Act
            var enabled = _character.GetEnabledFeatures().ToList();

            // Assert
            Assert.IsTrue(enabled.Contains(FeatureType.VipPromotion));
            Assert.IsTrue(enabled.Contains(FeatureType.DoiNangNo));
            Assert.IsFalse(enabled.Contains(FeatureType.TuHanh));
        }

        #endregion

        #region GetPendingFeatures Tests

        [TestMethod]
        public void GetPendingFeatures_ReturnsEnabledNotCompleted()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);
            _character.RuntimeState.MarkCompleted(FeatureType.VipPromotion);

            // Act
            var pending = _character.GetPendingFeatures().ToList();

            // Assert
            Assert.IsFalse(pending.Contains(FeatureType.VipPromotion)); // Completed
            Assert.IsTrue(pending.Contains(FeatureType.DoiNangNo)); // Pending
        }

        #endregion

        #region Id Property Tests

        [TestMethod]
        public void Id_Getter_ReturnsIdentityId()
        {
            // Assert
            Assert.AreEqual(_character.Identity.Id, _character.Id);
        }

        [TestMethod]
        public void Id_Setter_SetsIdentityId()
        {
            // Act
            _character.Id = "new-id";

            // Assert
            Assert.AreEqual("new-id", _character.Identity.Id);
        }

        #endregion

        #region ToString Tests

        [TestMethod]
        public void ToString_ReturnsIdentityString()
        {
            // Act
            var str = _character.ToString();

            // Assert
            Assert.IsTrue(str.Contains("test-char"));
        }

        #endregion
    }
}

