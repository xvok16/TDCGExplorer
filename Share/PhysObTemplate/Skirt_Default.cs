using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using TDCG;
using TDCGUtils;
using Tso2Pmd;

namespace TDCG.PhysObTemplate
{
    public class Skirt_Default : IPhysObTemplate
    {
        string name = "デフォルト"; // 枠に表示される名前
        int group = 2;              // 表示される枠の種類（0:髪 1:乳 2:スカート 3:その他）

        public string Name() { return name; }
        public int Group() { return group; }

        public void Execute(ref T2PPhysObjectList phys_list)
        {
            phys_list.MakeChain("左ス後１");
            phys_list.MakeChain("右ス後１");
            phys_list.MakeChain("右ス横１");
            phys_list.MakeChain("右ス前１");
            phys_list.MakeChain("左ス前１");
            phys_list.MakeChain("左ス横１");

            SetParameter(phys_list.GetBodyListByName(".ス.."));
            SetParameterSide(phys_list.GetBodyListByName(".ス横."));
            SetParameterEnd(phys_list.GetBodyListByName(".ス.先"));
            SetParameterSideEnd(phys_list.GetBodyListByName(".ス横先"));
            SetParameter(phys_list.GetJointListByName(".ス.."));

            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_1.X = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_2.X = (float)((60.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_1.Z = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").constrain_rot_2.Z = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").spring_rot.X = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス後１").spring_rot.Z = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_1.X = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_2.X = (float)((60.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_1.Z = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").constrain_rot_2.Z = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").spring_rot.X = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス後１").spring_rot.Z = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_1.X = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_2.X = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_1.Z = (float)((-15.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").constrain_rot_2.Z = (float)((60.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").spring_rot.X = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス横１").spring_rot.Z = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_1.X = (float)((-120.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_2.X = (float)((20.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_1.Z = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").constrain_rot_2.Z = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").spring_rot.X = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-右ス前１").spring_rot.Z = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_1.X = (float)((-120.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_2.X = (float)((20.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_1.Z = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").constrain_rot_2.Z = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").spring_rot.X = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス前１").spring_rot.Z = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_1.X = (float)((-5.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_2.X = (float)((5.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_1.Y = (float)((-0.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_2.Y = (float)((0.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_1.Z = (float)((-60.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").constrain_rot_2.Z = (float)((15.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").spring_rot.X = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").spring_rot.Y = 0.0f; // ばね：回転(rad(x), rad(y), rad(z))
            phys_list.GetJointByName("下半身-左ス横１").spring_rot.Z = 50.0f; // ばね：回転(rad(x), rad(y), rad(z))

            phys_list.MakeJointFromTwoBones("左ス前２", "右ス前２");
            phys_list.MakeJointFromTwoBones("右ス前２", "右ス横２");
            phys_list.MakeJointFromTwoBones("右ス横２", "右ス後２");
            phys_list.MakeJointFromTwoBones("右ス後２", "左ス後２");
            phys_list.MakeJointFromTwoBones("左ス後２", "左ス横２");
            phys_list.MakeJointFromTwoBones("左ス横２", "左ス前２");
            phys_list.MakeJointFromTwoBones("左ス前３", "右ス前３");
            phys_list.MakeJointFromTwoBones("右ス前３", "右ス横３");
            phys_list.MakeJointFromTwoBones("右ス横３", "右ス後３");
            phys_list.MakeJointFromTwoBones("右ス後３", "左ス後３");
            phys_list.MakeJointFromTwoBones("左ス後３", "左ス横３");
            phys_list.MakeJointFromTwoBones("左ス横３", "左ス前３");
        }

        // スカート前後
        private void SetParameter(List<PMD_RigidBody> body_list)
        {
            foreach (PMD_RigidBody body in body_list)
            {
                body.rigidbody_group_index = 4; // 諸データ：グループ // 00
                body.rigidbody_group_target = 1; // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
                body.shape_type = 1; // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
                body.shape_w = 0.8f; // 形状：半径(幅) // CD CC CC 3F // 1.6
                body.shape_h *= 0.6f; // 形状：高さ // CD CC CC 3D // 0.1
                body.shape_d = 0.2f; // 形状：奥行 // CD CC CC 3D // 0.1

                body.rigidbody_weight = 0.5f; // 諸データ：質量 // 00 00 80 3F // 1.0
                body.rigidbody_pos_dim = 0.5f; // 諸データ：移動減 // 00 00 00 00
                body.rigidbody_rot_dim = 0.9f; // 諸データ：回転減 // 00 00 00 00
                body.rigidbody_recoil = 0.0f; // 諸データ：反発力 // 00 00 00 00
                body.rigidbody_friction = 0.0f; // 諸データ：摩擦力 // 00 00 00 00
                body.rigidbody_type = 1; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
            }
        }

        // スカート横
        private void SetParameterSide(List<PMD_RigidBody> body_list)
        {
            foreach (PMD_RigidBody body in body_list)
            {
                body.rigidbody_group_index = 4; // 諸データ：グループ // 00
                body.rigidbody_group_target = 1; // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
                body.shape_type = 1; // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
                body.shape_w = 0.2f; // 形状：半径(幅) // CD CC CC 3F // 1.6
                body.shape_h *= 0.6f; // 形状：高さ // CD CC CC 3D // 0.1
                body.shape_d = 0.8f; // 形状：奥行 // CD CC CC 3D // 0.1

                body.rigidbody_weight = 0.5f; // 諸データ：質量 // 00 00 80 3F // 1.0
                body.rigidbody_pos_dim = 0.5f; // 諸データ：移動減 // 00 00 00 00
                body.rigidbody_rot_dim = 0.9f; // 諸データ：回転減 // 00 00 00 00
                body.rigidbody_recoil = 0.0f; // 諸データ：反発力 // 00 00 00 00
                body.rigidbody_friction = 0.0f; // 諸データ：摩擦力 // 00 00 00 00
                body.rigidbody_type = 1; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
            }
        }

        // スカート前後先
        private void SetParameterEnd(List<PMD_RigidBody> body_list)
        {
            foreach (PMD_RigidBody body in body_list)
            {
                body.rigidbody_group_index = 4; // 諸データ：グループ // 00
                body.rigidbody_group_target = 1; // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
                body.shape_type = 0; // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
                body.shape_w = 0.2f; // 形状：半径(幅) // CD CC CC 3F // 1.6

                body.rigidbody_weight = 0.5f; // 諸データ：質量 // 00 00 80 3F // 1.0
                body.rigidbody_pos_dim = 0.5f; // 諸データ：移動減 // 00 00 00 00
                body.rigidbody_rot_dim = 0.9f; // 諸データ：回転減 // 00 00 00 00
                body.rigidbody_recoil = 0.0f; // 諸データ：反発力 // 00 00 00 00
                body.rigidbody_friction = 0.0f; // 諸データ：摩擦力 // 00 00 00 00
                body.rigidbody_type = 1; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
            }
        }

        // スカート横先
        private void SetParameterSideEnd(List<PMD_RigidBody> body_list)
        {
            foreach (PMD_RigidBody body in body_list)
            {
                body.rigidbody_group_index = 4; // 諸データ：グループ // 00
                body.rigidbody_group_target = 1; // 諸データ：グループ：対象 // 0xFFFFとの差 // 38 FE
                body.shape_type = 0; // 形状：タイプ(0:球、1:箱、2:カプセル) // 00 // 球
                body.shape_w = 0.2f; // 形状：半径(幅) // CD CC CC 3F // 1.6

                body.rigidbody_weight = 0.5f; // 諸データ：質量 // 00 00 80 3F // 1.0
                body.rigidbody_pos_dim = 0.5f; // 諸データ：移動減 // 00 00 00 00
                body.rigidbody_rot_dim = 0.9f; // 諸データ：回転減 // 00 00 00 00
                body.rigidbody_recoil = 0.0f; // 諸データ：反発力 // 00 00 00 00
                body.rigidbody_friction = 0.0f; // 諸データ：摩擦力 // 00 00 00 00
                body.rigidbody_type = 1; // 諸データ：タイプ(0:Bone追従、1:物理演算、2:物理演算(Bone位置合せ)) // 00 // Bone追従
            }
        }

        private void SetParameter(List<PMD_Joint> joint_list)
        {
            foreach (PMD_Joint joint in joint_list)
            {
                joint.constrain_rot_1.X = (float)((-15.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
                joint.constrain_rot_2.X = (float)((15.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
                joint.constrain_rot_1.Z = (float)((-15.0 / 180.0) * Math.PI); // 制限：回転1(rad(x), rad(y), rad(z))
                joint.constrain_rot_2.Z = (float)((15.0 / 180.0) * Math.PI); // 制限：回転2(rad(x), rad(y), rad(z))
                joint.spring_rot.X = 20.0f; // ばね：回転(rad(x), rad(y), rad(z))
                joint.spring_rot.Y = 20.0f; // ばね：回転(rad(x), rad(y), rad(z))
                joint.spring_rot.Z = 20.0f; // ばね：回転(rad(x), rad(y), rad(z))
            }
        }
    }
}
