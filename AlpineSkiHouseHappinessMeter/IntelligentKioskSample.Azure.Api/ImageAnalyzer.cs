﻿// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: http://www.microsoft.com/cognitive
// 
// Microsoft Cognitive Services Github:
// https://github.com/Microsoft/Cognitive
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

//using Microsoft.ProjectOxford.Common.Contract;
using ServiceHelpers.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceHelpers
{
    public class ImageAnalyzer
    {
        public event EventHandler EmotionRecognitionCompleted;

        public Func<Task<Stream>> GetImageStreamCallback { get; set; }
        //public string ImageUrl { get; set; }

        //Implement : You should declare a property, Task 4, Step 1
        public IList<FaceEmotionData> DetectedEmotion { get; set; }


        // Default to no errors, since this could trigger a stream of popup errors since we might call this
        // for several images at once while auto-detecting the Bing Image Search results.
        public bool ShowDialogOnFaceApiErrors { get; set; } = false;
        public bool FilterOutSmallFaces { get; set; } = false;
        public int DecodedImageHeight { get; private set; }
        public int DecodedImageWidth { get; private set; }
        public byte[] Data { get; set; }

        //public ImageAnalyzer(string url)
        //{
        //    this.ImageUrl = url;
        //}

        public ImageAnalyzer(byte[] data)
        {
            this.Data = data;
            this.GetImageStreamCallback = () => Task.FromResult<Stream>(new MemoryStream(this.Data));
        }

        public void UpdateDecodedImageSize(int height, int width)
        {
            this.DecodedImageHeight = height;
            this.DecodedImageWidth = width;
        }

        public async Task DetectEmotionAsync()
        {
            try
            {
                // Implement #1: If there is ImageUrl you should call the proper EmotionServiceHelper method to detect emotions
                //if (this.ImageUrl != null)
                //{
                //    //this.DetectedEmotion = await EmotionServiceHelper.RecognizeAsync(this.ImageUrl);
                //    throw new NotImplementedException();
                //}
                // Implement #2: If GetImageStreamCallback is not null, you should call the proper EmotionServiceHelper method to detect emotions
                //else 
                if (this.GetImageStreamCallback != null)
                {
                    this.DetectedEmotion = await EmotionServiceHelper.RecognizeAsync(this.GetImageStreamCallback);
                }

                // Implement #3: If FilterOutSmallFaces is enabled, filter the DetectedEmotion using the CoreUtil IsFaceBigEnoughForDetection method results
                if (this.FilterOutSmallFaces)
                {
                    this.DetectedEmotion = this.DetectedEmotion.Where(f => CoreUtil.IsFaceBigEnoughForDetection(f.FaceRectangle.Height, this.DecodedImageHeight)).ToList();
                }
            }
            catch (Exception e)
            {
                // Implement #4: If there is an error, call the ErrorTrackingHelper helper class to record the issue.
                //               and return an empty emotion list
                ErrorTrackingHelper.TrackException(e, "Emotion API RecognizeAsync error");

                //this.DetectedEmotion = Enumerable.Empty<Emotion>();
                this.DetectedEmotion = null;

                if (this.ShowDialogOnFaceApiErrors)
                {
                    await ErrorTrackingHelper.GenericApiCallExceptionHandler(e, "Emotion detection failed.");
                }

#if DEBUG
                throw e;
#endif
            }
            finally
            {
                // Implement #5: Call the event OnEmotionRecognitionCompleted
                this.OnEmotionRecognitionCompleted();
            }
        }

        private void OnEmotionRecognitionCompleted()
        {
            if (this.EmotionRecognitionCompleted != null)
            {
                this.EmotionRecognitionCompleted(this, EventArgs.Empty);
            }
        }
    }
}
