﻿using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using AspectCore.DynamicProxy;


namespace AopCaching.Core
{
	[NonAspect]
	public class DefaultCacheKeyGenerator : ICacheKeyGenerator
	{
		private const string LinkString = ":";

		public string GeneratorKey(MethodInfo methodInfo, object[] args, string customKey = "", string prefix = "", bool shortKey = false)
		{
			var attribute =
				methodInfo.GetCustomAttributes(true).FirstOrDefault(p => p.GetType() == typeof(AopCachingAttribute))
					as AopCachingAttribute;
			if (attribute == null || string.IsNullOrWhiteSpace(attribute.Key))
			{
				var typeName = methodInfo.DeclaringType?.FullName;
				var methodName = methodInfo.Name;
				var generics = methodInfo.GetGenericArguments();

				if (shortKey)
					return
						MD5($"{typeName}{LinkString}{methodName}{(generics.Any() ? LinkString : "")}{(generics.Any() ? $"<{string.Join(",", generics.Select(p => p))}>" : "")}{(args.Any() ? LinkString : "")}{(args.Any() ? DataSerializer.ToJson(args) : "")}");
				return
					$"{(string.IsNullOrWhiteSpace(prefix) ? "" : $"{prefix}{LinkString}")}{typeName}{LinkString}{methodName}{(generics.Any() ? LinkString : "")}{(generics.Any() ? $"<{string.Join(",", generics.Select(p => p))}>" : "")}{(args.Any() ? LinkString : "")}{(args.Any() ? MD5(DataSerializer.ToJson(args)) : "")}";
			}
			return string.IsNullOrWhiteSpace(prefix) ? "" : $"{prefix}{LinkString}" + string.Format(attribute.Key, args);
		}

		private string MD5(string source)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(source);
			using (MD5 md5 = new MD5CryptoServiceProvider())
			{
				var hash = md5.ComputeHash(bytes);
				md5.Clear();
				return BitConverter.ToString(hash).Replace("-", "").ToLower();
			}
		}
	}
}
