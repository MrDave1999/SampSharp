﻿// SampSharp
// Copyright 2020 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.Helpers;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.SAMP.Commands;
using SampSharp.GameMode.World;

namespace TestMode
{
    internal class Commands
    {
        [Command("kickme")]
        public static async void KickMeCommand(BasePlayer player)
        {
            player.SendClientMessage("Bye!");
            await Task.Delay(10);
            player.Kick();
        }

        [Command("quattest")]
        public static void QuatTestCommand(BasePlayer player)
        {
            var v = player.Vehicle;
            if (v == null) return;

            var q1 = v.GetRotationQuat();
            var m = Matrix.CreateFromQuaternion(q1);
            var q2 = m.Rotation;

            player.SendClientMessage("1: " + q1.ToVector4());
            player.SendClientMessage("2: " + q2.ToVector4());
        }

        [Command("rear")]
        public static async void RearCommand(BasePlayer player)
        {
            var labels = new List<TextLabel>();

            foreach (var vehicle in BaseVehicle.All)
            {
                var model = vehicle.Model;

                var size = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.Size);
                var bumper = BaseVehicle.GetModelInfo(model, VehicleModelInfoType.RearBumperZ);
                var offset = new Vector3(0, -size.Y / 2, bumper.Z);

                var rotation = vehicle.GetRotationQuat();

                var mRotation = rotation.LengthSquared > 10000 // Unoccupied vehicle updates corrupt the internal vehicle world matrix
                    ? Matrix.CreateRotationZ(MathHelper.ToRadians(vehicle.Angle))
                    : Matrix.CreateFromQuaternion(rotation);

                var matrix = Matrix.CreateTranslation(offset) *
                             mRotation *
                             Matrix.CreateTranslation(vehicle.Position);

                var point = matrix.Translation;

                labels.Add(new TextLabel("[x]", Color.Blue, point, 100, 0, false));
            }
            
            await Task.Delay(10000);

            foreach(var l in labels)
                l.Dispose();
            
        }
        
        [Command("spawnat")]
        public static void SpawnCommand(BasePlayer player, VehicleModelType type, float x, float y, float z, float a)
        {
            BaseVehicle.Create(type, new Vector3(x, y, z), a, -1, -1);
        }

        [Command("spawn")]
        public static void SpawnCommand(BasePlayer player, VehicleModelType type)
        {
            var vehicle = BaseVehicle.Create(type, player.Position + Vector3.Up, player.Angle, -1, -1);
            player.PutInVehicle(vehicle);
            vehicle.GetDamageStatus(out var panels, out var doors, out var lights, out var tires);
            Console.WriteLine(panels.ToString());
            Console.WriteLine(doors.ToString());
            Console.WriteLine(lights.ToString());
            Console.WriteLine(tires.ToString());
        }

        [Command("status")]
        public static void StatusCommand(BasePlayer player, int vehicleid)
        {
            var vehicle = BaseVehicle.Find(vehicleid);
            var panels = 99;
            var doors = 100;
            var lights = 101;
            var tires = 102;
            vehicle.GetDamageStatus(out panels, out doors, out lights, out tires);
            Console.WriteLine(panels.ToString());
            Console.WriteLine(doors.ToString());
            Console.WriteLine(lights.ToString());
            Console.WriteLine(tires.ToString());
        }

        [Command("setstatus")]
        public static void SetStatusCommand(BasePlayer player, int vehicleid)
        {
            var vehicle = BaseVehicle.Find(vehicleid);
            vehicle.SetDoorsParameters(true, true, true, true);

            vehicle.GetDamageStatus(out var panels, out var doors, out var lights, out var tires);
            Console.WriteLine(panels.ToString());
            Console.WriteLine(doors.ToString());
            Console.WriteLine(lights.ToString());
            Console.WriteLine(tires.ToString());
        }

        [Command("give")]
        public static void GiveCommand(BasePlayer player, Weapon weapon, int ammo)
        {
            player.GiveWeapon(weapon, ammo);
        }

        [Command("enter")]
        public static void EnterCommand(BasePlayer player, BaseVehicle vehicle)
        {
            player.PutInVehicle(vehicle);
        }

        [Command("myfirstcommand")]
        public static void MyFirstCommand(BasePlayer player, string message)
        {
            player.SendClientMessage($"Hello, world! You said {message}");
        }

        [Command("pos")]
        public static async void PositionCommand(BasePlayer player)
        {
            player.SendClientMessage(Color.Yellow, $"Position: {player.Position}");

            await Task.Delay(1000);

            player.SendClientMessage("Still here!");
        }

        [Command("dialogtest")]
        public static async void DialogTest(BasePlayer player)
        {
            var dialog = new MessageDialog("Test dialog", "This message should hide in 2 seconds.", "Don't click me!");
            dialog.Response += (sender, args) =>
            {
                player.SendClientMessage("You responed to the dialog with button" + args.DialogButton);
            };

            player.SendClientMessage("Showing dialog");
            dialog.Show(player);

            await Task.Delay(2000);

            player.SendClientMessage("Hiding dialog");
            Dialog.Hide(player);
        }

        [Command("asyncdialog")]
        public static async void DialogAsyncTest(BasePlayer player)
        {
            var dialog = new MessageDialog("Async dialog test", "Quit with this dialog still open.", "Don't click me!");

            Console.WriteLine("Showing dialog");
            try
            {
                await dialog.ShowAsync(player);
                Console.WriteLine("Dialog ended");
            }
            catch (PlayerDisconnectedException e)
            {
                Console.WriteLine($"{player} left.");
                Console.WriteLine(e);
            }
        }

        [Command("weapon")]
        public static void WeaponCommand(BasePlayer player, Weapon weapon, int ammo = 30)
        {
            player.GiveWeapon(weapon, ammo);
        }

        [Command("kick")]
        public static void Kick(BasePlayer player, BasePlayer target)
        {
            target.Kick();
        }

        [Command("help")]
        private static void Help(BasePlayer player)
        {
            player.SendClientMessage("/reverse, /help");
        }

        [Command("reverse")]
        private static void Reverse(BasePlayer player, string message)
        {
            player.SendClientMessage($"{message} reversed: ");
            message = new string(message.Reverse().ToArray());
            player.SendClientMessage(message);
        }
    }
}