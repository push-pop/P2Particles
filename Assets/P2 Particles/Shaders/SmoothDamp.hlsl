float3 SmoothDamp (float current, float target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
 {
     smoothTime = Mathf.Max (0.0001f, smoothTime);
     float num = 2f / smoothTime;
     float num2 = num * deltaTime;
     float num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
     float num4 = current - target;
     float num5 = target;
     float num6 = maxSpeed * smoothTime;
     num4 = Mathf.Clamp (num4, -num6, num6);
     target = current - num4;
     float num7 = (currentVelocity + num * num4) * deltaTime;
     currentVelocity = (currentVelocity - num * num7) * num3;
     float num8 = target + (num4 + num7) * num3;
     if (num5 - current > 0f == num8 > num5)
     {
         num8 = num5;
         currentVelocity = (num8 - num5) / deltaTime;
     }
     return num8;
 }