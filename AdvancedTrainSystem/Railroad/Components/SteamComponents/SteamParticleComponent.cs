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

        private DynamoComponent _dynamo;
        private BoilerComponent _boiler;
        private SafetyValveComponent _safetyValve;

        public SteamParticleComponent(ComponentCollection components) : base(components)
        {

        }

        public override void Start()
        {
            base.Start();
            _boiler = Components.GetComponent<BoilerComponent>();
            _dynamo = Components.GetComponent<DynamoComponent>();
            _safetyValve = Components.GetComponent<SafetyValveComponent>();

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

            _dynamoSteam.Play();
            _funnelSmoke.Play();
        }

        public override void Update()
        {
            base.Update();

            // TODO: Improve cylinder & funnel smoke simulation

            _dynamoSteam.Size = _dynamo.Output;
            _cylinderSteam.SetState(_boiler.Pressure > 0.3f);

            _safetyValveSteam.SetState(_safetyValve.Valve > 0.05f);
            _safetyValveSteam.Size = _safetyValve.Valve / 4;

            float drainCockSize = _controls.DrainCocks;
            drainCockSize *= _boiler.Pressure;

            _drainCockSteam.SetState(_controls.DrainCocks > 0.05f && _boiler.Pressure > 0.1f);
            _drainCockSteam.Interval = 250;
            _drainCockSteam.Size = drainCockSize;

            //_funnelSmoke.Color = System.Drawing.Color.FromArgb(45, 45, 45);
        }

        public override void Dispose()
        {
            base.Dispose();

            _dynamoSteam.Dispose();
            _cylinderSteam.Dispose();
            _funnelSmoke.Dispose();
            _safetyValveSteam.Dispose();
            _drainCockSteam.Dispose();
        }
    }
}
