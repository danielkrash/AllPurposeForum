using AllPurposeForum; // For MLModelNewAttempt and its inner classes
using FluentAssertions;
using Xunit;
using System.Linq; // For Sum()
using System.Collections.Generic; // For KeyValuePair

namespace AllPurposeForum.ML.Test
{
    public class MLModelNewAttemptTests
    {
        // IMPORTANT: Ensure the MLModelNewAttempt.mlnet file is copied to the test's output directory.
        // This can typically be done by selecting the .mlnet file in the main project (AllPurposeForum)
        // and setting its "Copy to Output Directory" property to "Copy if newer" or "Copy always".

        [Fact]
        public void Predict_Should_Return_Prediction_For_Sample_Input()
        {
            // Arrange
            var input = new MLModelNewAttempt.ModelInput
            {
                Sentence = @"contains no wit , only labored gags"
                // Label is not strictly needed for prediction, but it's part of ModelInput
            };

            // Act
            MLModelNewAttempt.ModelOutput output = null;
            try
            {
                output = MLModelNewAttempt.Predict(input);
            }
            catch (System.Exception ex)
            {
                // Catch exception during model loading/prediction to provide more info
                throw new System.Exception($"MLModelNewAttempt.Predict failed. Ensure 'MLModelNewAttempt.mlnet' is in the correct path and readable. Original exception: {ex.Message}", ex);
            }

            // Assert
            output.Should().NotBeNull();
            output.Sentence.Should().Be(input.Sentence); // The model output includes the input sentence

            // Assuming binary classification (e.g., positive/negative)
            // PredictedLabel is often 0 or 1.
            // The actual value depends on the model's training.
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
            var input = new MLModelNewAttempt.ModelInput
            {
                Sentence = "This is terrible, I hate it."
            };

            // Act
            MLModelNewAttempt.ModelOutput output = null;
            try
            {
                output = MLModelNewAttempt.Predict(input);
            }
            catch (System.Exception ex)
            {
                throw new System.Exception($"MLModelNewAttempt.Predict failed. Ensure 'MLModelNewAttempt.mlnet' is in the correct path and readable. Original exception: {ex.Message}", ex);
            }

            // Assert
            output.Should().NotBeNull();
            output.Sentence.Should().Be(input.Sentence);

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
            var input = new MLModelNewAttempt.ModelInput
            {
                Sentence = "This is a neutral statement."
            };

            // Act
            IOrderedEnumerable<KeyValuePair<string, float>> labelledScores = null;
            try
            {
                labelledScores = MLModelNewAttempt.PredictAllLabels(input);
            }
            catch (System.Exception ex)
            {
                 throw new System.Exception($"MLModelNewAttempt.PredictAllLabels failed. Ensure 'MLModelNewAttempt.mlnet' is in the correct path and readable, and the schema contains the 'label' column. Original exception: {ex.Message}", ex);
            }

            // Assert
            labelledScores.Should().NotBeNull();
            // Assuming binary classification, so two labels ("0", "1") are expected.
            // If your model has more labels, adjust this count.
            labelledScores.Should().HaveCount(2, "assuming binary classification, so two labels are expected."); 

            float previousScore = float.MaxValue;
            foreach (var pair in labelledScores)
            {
                pair.Key.Should().NotBeNullOrEmpty(); // Label name (e.g., "0" or "1")
                pair.Value.Should().BeInRange(0.0f, 1.0f); // Score
                pair.Value.Should().BeLessThanOrEqualTo(previousScore); // Check descending order
                previousScore = pair.Value;
            }

            labelledScores.Sum(kvp => kvp.Value).Should().BeApproximately(1.0f, 0.0001f, "sum of scores should be approx 1.0");
        }
    }
}
