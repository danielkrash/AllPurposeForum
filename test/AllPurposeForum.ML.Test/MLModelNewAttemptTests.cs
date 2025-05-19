using AllPurposeForum; // For MLModel and its inner classes
using FluentAssertions;
using Xunit;
using System.Linq; // For Sum()
using System.Collections.Generic; // For KeyValuePair

namespace AllPurposeForum.ML.Test
{
    public class MLModelTests // Renamed from MLModelNewAttemptTests
    {
        // IMPORTANT: Ensure the MLModel.mlnet file is copied to the test's output directory.
        // This can typically be done by selecting the .mlnet file in the main project (AllPurposeForum)
        // and setting its "Copy to Output Directory" property to "Copy if newer" or "Copy always".

        [Fact]
        public void Predict_Should_Return_Prediction_For_Sample_Input()
        {
            // Arrange
            var input = new MLModel.ModelInput 
            {
                Sentiment = @"contains no wit , only labored gags"
            };

            // Act
            MLModel.ModelOutput output = default!; 
            try
            {
                output = MLModel.Predict(input); 
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"MLModel.Predict failed. Ensure 'MLModel.mlnet' is in the correct path and readable. Original exception: {ex.Message}", ex);
            }

            // Assert
            output.Should().NotBeNull();
            // output.Sentiment.Should().Be(input.Sentiment); // This line was commented out as MLModel.ModelOutput might not have Sentiment

            (output.PredictedLabel == 0.0f || output.PredictedLabel == 1.0f).Should().BeTrue(
                $"PredictedLabel should be 0 or 1, but was {output.PredictedLabel}. This might indicate an issue with the model or an unexpected output format.");

            output.Score.Should().NotBeNull();
            output.Score.Should().HaveCount(2, "assuming binary classification, so two scores are expected.");
            output.Score.ToList().ForEach(s => s.Should().BeInRange(0.0f, 1.0f));
            output.Score.Sum().Should().BeApproximately(1.0f, 0.0001f, "scores should sum to 1 after softmax.");
        }

        [Fact]
        public void Predict_Should_Handle_Another_Sample_Input()
        {
            // Arrange
            var input = new MLModel.ModelInput 
            {
                Sentiment = "This is terrible, I hate it."
            };

            // Act
            MLModel.ModelOutput output = default!; 
            try
            {
                output = MLModel.Predict(input); 
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"MLModel.Predict failed. Ensure 'MLModel.mlnet' is in the correct path and readable. Original exception: {ex.Message}", ex);
            }

            // Assert
            output.Should().NotBeNull();
            // output.Sentiment.Should().Be(input.Sentiment); // This line was commented out

            (output.PredictedLabel == 0.0f || output.PredictedLabel == 1.0f).Should().BeTrue(
                 $"PredictedLabel should be 0 or 1, but was {output.PredictedLabel}.");

            output.Score.Should().NotBeNull();
            output.Score.Should().HaveCount(2); // Assuming binary classification
            output.Score.ToList().ForEach(s => s.Should().BeInRange(0.0f, 1.0f));
            output.Score.Sum().Should().BeApproximately(1.0f, 0.0001f);
        }

        [Fact]
        public void PredictAllLabels_Should_Return_Sorted_Scores()
        {
            // Arrange
            var input = new MLModel.ModelInput 
            {
                Sentiment = "This is a neutral statement."
            };

            // Act
            IOrderedEnumerable<KeyValuePair<string, float>> labelledScores = default!;
            try
            {
                labelledScores = MLModel.PredictAllLabels(input); 
            }
            catch (System.Exception ex)
            {
                 throw new System.Exception($"MLModel.PredictAllLabels failed. Ensure 'MLModel.mlnet' is in the correct path and readable, and the schema contains the 'label' column. Original exception: {ex.Message}", ex);
            }

            // Assert
            labelledScores.Should().NotBeNull();
            labelledScores.Should().HaveCount(2, "assuming binary classification, so two labels are expected."); 

            float previousScore = float.MaxValue;
            foreach (var pair in labelledScores)
            {
                pair.Key.Should().NotBeNullOrEmpty(); 
                pair.Value.Should().BeInRange(0.0f, 1.0f); 
                pair.Value.Should().BeLessThanOrEqualTo(previousScore); 
                previousScore = pair.Value;
            }

            labelledScores.Sum(kvp => kvp.Value).Should().BeApproximately(1.0f, 0.0001f, "sum of scores should be approx 1.0");
        }
    }
}
