using AdvancedTrainSystem.Core.Components;
using AdvancedTrainSystem.Core.Components.Abstract;
using AdvancedTrainSystem.Core.Utils;
using FusionLibrary;
using FusionLibrary.Extensions;
using GTA;
using GTA.Math;
using RageComponent.Core;
using static FusionLibrary.FusionEnums;

namespace AdvancedTrainSystem.Railroad.Components.SteamComponents
{
    public class SteamParticleComponent : ParticleComponent
    {
        private readonly ParticlePlayerHandler _cylinderSteam = new ParticlePlayerHandler();
        private readonly ParticlePlayerHandler _drainCockSteam = new ParticlePlayerHandler();
        private ParticlePlayer _funnelSmoke;
        private ParticlePlayer _dynamoSteam;
        private ParticlePlayer _safetyValveSteam;
        private ParticlePlayer _fireboxFire;

        private DynamoComponent _dynamo;
        private BoilerComponent _boiler;
        private SafetyValveComponent _safetyValve;
        private ChimneyComponent _chimney;
        private DerailComponent _derail;

        private Prop _fireboxDummyProp;

        public SteamParticleComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();
            _boiler = Components.GetComponent<BoilerComponent>();
            _dynamo = Components.GetComponent<DynamoComponent>();
            _safetyValve = Components.GetComponent<SafetyValveComponent>();
            _chimney = Components.GetComponent<ChimneyComponent>();
            _derail = Components.GetComponent<DerailComponent>();

            CustomModel dummyModel = new CustomModel("prop_oil_valve_01");
            dummyModel.Request();

            // Dummy prop we attach some particles to in order to remove
            // physics, for example smoke or fire needs to be static
            // no matter train moves or not
            _fireboxDummyProp = World.CreateProp(dummyModel, train.Position, false, false);
            _fireboxDummyProp.IsPersistent = true;
            _fireboxDummyProp.IsVisible = false;
            _fireboxDummyProp.IsPositionFrozen = true;
            _fireboxDummyProp.IsCollisionEnabled = true;

            _derail.OnDerail += () =>
            {
                _fireboxDummyProp.Delete();
            };

            // Cylinder smoke and drips
            BoneUtils.ProcessSideBones(
                baseBone: "boiler_steam_", 
                totalBoneNumber: 4, 
                action: bone =>
            {
                // Smoke
                _cylinderSteam.Add(
                    assetName: "cut_pacific_fin",
                    effectName: "cs_pac_fin_skid_smoke",
                    particleType: ParticleType.Looped,
                    entity: train,
                    boneName: bone,
                    offset: new Vector3(0, 0, 0.4f),
                    rotation: new Vector3(-90, 0, 0));

                // Cock drips
                _cylinderSteam.Add(
                    assetName: "scr_apartment_mp",
                    effectName: "scr_apa_jacuzzi_drips",
                    particleType: ParticleType.Looped,
                    entity: train,
                    boneName: bone,
                    size: 3f);
            });

            // Dynamo
            _dynamoSteam = new ParticlePlayer(
                assetName: "scr_gr_bunk",
                effectName: "scr_gr_bunk_drill_smoke",
                particleType: ParticleType.Looped,
                entity: train,
                boneName: "dynamo_steam");

            // Funnel
            _funnelSmoke = new ParticlePlayer(
                assetName: "scr_carsteal4", 
                effectName: "scr_carsteal4_wheel_burnout", 
                particleType: ParticleType.ForceLooped, 
                entity: train, 
                boneName: "funnel_smoke",
                offset: new Vector3(0, -0.58f, 0), 
                rotation: new Vector3(90, 0, 0))
            {
                Size = 0.5f,
                Interval = 55
            };

            // Safety Valve
            _safetyValveSteam = new ParticlePlayer(
                assetName: "core",
                effectName: "ent_sht_steam",
                particleType: ParticleType.Looped,
                entity: train,
                boneName: "funnel_smoke",
                offset: new Vector3(0, -3.4f, -0.3f));

            // Drain cock blasts
            BoneUtils.ProcessSideBones(
                baseBone: "boiler_steam_",
                totalBoneNumber: 4,
                action: bone =>
                {
                    Vector3 rot = bone.Contains("right") ? new Vector3(0, 0, 180) : Vector3.Zero;

                    // Smoke
                    _drainCockSteam.Add(
                        assetName: "core",
                        effectName: "ent_sht_steam",
                        particleType: ParticleType.ForceLooped,
                        entity: train,
                        boneName: bone,
                        rotation: new Vector3(0, -90, 0) + rot);
                });

            // Fire
            // There's something really fucked up
            // about rotation and position
            // please don't change it
            _fireboxFire = new ParticlePlayer(
                assetName: "core",
                effectName: "ent_amb_barrel_fire",
                particleType: ParticleType.Looped,
                entity: _fireboxDummyProp,
                offset: new Vector3(0f, 0.3f, -0.3f),
                rotation: new Vector3(60, 0, 0));

            _dynamoSteam.Play();
            _funnelSmoke.Play();
        }

        public override void Update()
        {
            base.Update();

            // TODO: Improve cylinder

            _dynamoSteam.Size = _dynamo.Output;
            _cylinderSteam.SetState(_boiler.Pressure > 0.3f);

            _safetyValveSteam.SetState(_safetyValve.Valve > 0.05f);
            _safetyValveSteam.Size = _safetyValve.Valve / 4;

            float drainCockSize = _controls.DrainCocks;
            drainCockSize *= _boiler.Pressure;

            _drainCockSteam.SetState(_controls.DrainCocks > 0.05f && _boiler.Pressure > 0.1f);
            _drainCockSteam.Interval = 250;
            _drainCockSteam.Size = drainCockSize;

            int funnelColor = (int) _chimney.AirInBoiler.Remap(0f, 1f, 65, 255);
            // More air in boiler - lighter smoke
            _funnelSmoke.Color = System.Drawing.Color.FromArgb(funnelColor, funnelColor, funnelColor);

            // More air in boiler - less fuel burning, so we make less smoke appear
            _funnelSmoke.Interval = (int)_chimney.AirInBoiler.Remap(0f, 1f, 55, 500);

            // For some reason otherwise it makes train fly on derail...
            if(!_derail.IsDerailed)
            {
                // Enable and set size of firebox fire depending on how many coal there is
                _fireboxDummyProp.Position = train.Bones["firebox_fire"].Position;
                _fireboxDummyProp.Rotation = train.Rotation;

                _fireboxFire.SetState(_chimney.AirInBoiler < 0.8f);
                _fireboxFire.Size = 1 - _chimney.AirInBoiler;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _dynamoSteam.Dispose();
            _cylinderSteam.Dispose();
            _funnelSmoke.Dispose();
            _safetyValveSteam.Dispose();
            _drainCockSteam.Dispose();
            _fireboxFire.Dispose();

            _fireboxDummyProp.Delete();
        }
    }
}
