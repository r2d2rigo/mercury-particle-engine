﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mercury.ParticleEngine {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Mercury.ParticleEngine.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Texture2D&lt;float4&gt; Texture : register(t0);
        ///sampler TextureSampler : register(s0);
        ///
        ///cbuffer Parameters : register(b0)
        ///{
        ///	float4x4 WVP : packoffset(c0);
        ///	bool FastFade : packoffset(c4.x);
        ///};
        ///
        ///struct VS_IN
        ///{
        ///	float2 pos : POSITION;
        ///	float4 color : COLOR0;
        ///	float2 tex : TEXCOORD;
        ///	float age : COLOR1;
        ///};
        ///
        ///struct PS_IN
        ///{
        ///	float4 pos : SV_POSITION;
        ///	float4 color : COLOR0;
        ///	float2 tex : TEXCOORD;
        ///};
        ///
        ///float3 HueToRgb(in float hue)
        ///{
        ///	float r = abs(hue * 6 - 3) - 1;
        ///	float g = 2 - abs(hue * 6 - [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SpriteBatchShader {
            get {
                return ResourceManager.GetString("SpriteBatchShader", resourceCulture);
            }
        }
    }
}
