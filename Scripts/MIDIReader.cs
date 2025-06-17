using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine.Assertions;
using System;
using static UnityEditor.Experimental.GraphView.Port;

// Reads and parses MIDI files, and triggers animation events.
public class MIDIReader : MonoBehaviour
{
    // Change endianness of 32-bit value
    uint change_endian_32(uint value)
    {
        return (value & 0xff000000) >> 24
        | (value & 0x00ff0000) >> 8
        | (value & 0x0000ff00) << 8
        | (value & 0x000000ff) << 24;
    }
    // Change endianness of 16-bit value
    uint change_endian_16(uint value)
    {
        return
         (value & 0xff00) >> 8
        | (value & 0x00ff) << 8;
    }
    public string file_name;
    public AudioSource au;
    public List<List<int[]>> EventList = new List<List<int[]>>();
    uint measure = 0;
    int track_now;
    public List<float> current_time = new List<float>();
    public List<int> e_num = new List<int>();
    public GameObject[,] OList = new GameObject[256, 256];
    public GameObject[] PList = new GameObject[256];
    // Start is called before the first frame update
    void Start()
    {
        FileStream f = new FileStream("Assets/" + file_name + ".mid", System.IO.FileMode.Open, FileAccess.Read);
        BinaryReader reader = new BinaryReader(f);
        try
        {
            string log = "LOADING...";
            for (int i = 0; i < f.Length;)
            {
                log += "\nCHUNK";
                i += 4;
                uint chunk = change_endian_32(reader.ReadUInt32());
                if (chunk == 0x4D546864)
                {
                    log += "\nheader begin...";
                    uint header_length = change_endian_32(reader.ReadUInt32());
                    i += 4;
                    log += "\nheader_length=" + header_length;
                    uint format = change_endian_16(reader.ReadUInt16());
                    i += 2;
                    log += "\nformat=" + format;
                    uint track_num = change_endian_16(reader.ReadUInt16());
                    i += 2;
                    log += "\ntrack_num=" + track_num;
                    measure = change_endian_16(reader.ReadUInt16());
                    i += 2;
                    log += "\nmeasure=" + measure;
                }
                else
                {
                    EventList.Add(new List<int[]>());
                    track_now++;
                    log += "\ntrack begin..." + track_now;
                    uint track_length = change_endian_32(reader.ReadUInt32());
                    i += 4;
                    log += "\ntrack_length=" + track_length;
                    byte running = 0;
                    for (int ii = 0; ii < track_length;)
                    {
                        // Variable-length value begin
                        int deltatime = 0;
                        byte b1 = reader.ReadByte();
                        i += 1;
                        ii += 1;
                        if (b1 <= 127)
                        {
                            deltatime = b1;
                        }
                        else
                        {
                            byte b2 = reader.ReadByte();
                            i += 1;
                            ii += 1;
                            if (b2 <= 127)
                            {
                                deltatime = 128 * (b1 - 128) + b2;
                            }
                            else
                            {
                                byte b3 = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                if (b3 <= 127)
                                {
                                    deltatime = 128 * 128 * (b1 - 128) + 128 * (b2 - 128) + b3;
                                }
                                else
                                {
                                    byte b4 = reader.ReadByte();
                                    i += 1;
                                    ii += 1;
                                    if (b4 <= 127)
                                    {
                                        deltatime = 128 * 128 * 128 * (b1 - 128) + 128 * 128 * (b2 - 128) + 128 * (b3 - 128) + b4;
                                    }
                                }
                            }
                        }
                        // Variable-length value end
                        log += "\ndeltatime=" + deltatime;
                        byte t_event = reader.ReadByte();
                        i += 1;
                        ii += 1;
                        int ch_num = 0;
                        byte note_num = 0;
                        byte velocity = 0;
                        byte mystery = 0;
                        switch (t_event / 16)
                        {
                            case 8:
                                running = t_event;
                                log += " note_off";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " note_num=" + note_num;
                                velocity = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " velocity=" + velocity;
                                EventList[track_now - 1].Add(new int[] { ch_num, deltatime, note_num, 1,velocity});
                                break;
                            case 9:
                                running = t_event;
                                log += " note_on";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " note_num=" + note_num;
                                velocity = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " velocity=" + velocity;
                                EventList[track_now - 1].Add(new int[] { ch_num, deltatime, note_num, 0, velocity });
                                break;
                            case 10:
                                running = t_event;
                                log += " key_after_touch";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " note_num=" + note_num;
                                velocity = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " velocity=" + velocity;
                                break;
                            case 11:
                                running = t_event;
                                log += " ctrl_change";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " note_num=" + note_num;
                                velocity = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " velocity=" + velocity;
                                break;
                            case 12:
                                running = t_event;
                                log += " program_change";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " change_num=" + note_num;
                                break;
                            case 13:
                                running = t_event;
                                log += " chanel_after_touch";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " velocity=" + note_num;
                                break;
                            case 14:
                                running = t_event;
                                log += " pitch_bend";
                                ch_num = t_event % 16;
                                log += " ch_num=" + ch_num;
                                note_num = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " mean=" + note_num;
                                velocity = reader.ReadByte();
                                i += 1;
                                ii += 1;
                                log += " range=" + velocity;
                                break;
                            case 15:
                                running = t_event;
                                log += " extra_event=" + t_event + " ";
                                switch (t_event % 16)
                                {
                                    case 0:
                                        while (true)
                                        {
                                            mystery = reader.ReadByte();
                                            log += Convert.ToString(mystery, 16) + " ";
                                            i += 1;
                                            ii += 1;
                                            if (mystery == 247) break;
                                        }
                                        log += " END";
                                        break;
                                    case 1:
                                        mystery = reader.ReadByte();
                                        log += " time=";
                                        log += Convert.ToString(mystery, 16);
                                        i += 1;
                                        ii += 1;
                                        break;
                                    case 2:
                                        mystery = reader.ReadByte();
                                        log += " time1=";
                                        log += Convert.ToString(mystery, 16);
                                        i += 1;
                                        ii += 1;
                                        mystery = reader.ReadByte();
                                        log += " time2=";
                                        log += Convert.ToString(mystery, 16);
                                        i += 1;
                                        ii += 1;
                                        break;
                                    case 3:
                                        mystery = reader.ReadByte();
                                        log += " songnum=";
                                        log += Convert.ToString(mystery, 16);
                                        i += 1;
                                        ii += 1;
                                        break;
                                    case 15:
                                        mystery = reader.ReadByte();
                                        log += " meta_type=";
                                        log += Convert.ToString(mystery, 16);
                                        i += 1;
                                        ii += 1;
                                        // Variable-length value begin
                                        int len = 0;
                                        byte l1 = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        if (l1 <= 127)
                                        {
                                            len = l1;
                                        }
                                        else
                                        {
                                            byte l2 = reader.ReadByte();
                                            i += 1;
                                            ii += 1;
                                            if (l2 <= 127)
                                            {
                                                len = 128 * (l1 - 128) + l2;
                                            }
                                            else
                                            {
                                                byte l3 = reader.ReadByte();
                                                i += 1;
                                                ii += 1;
                                                if (l3 <= 127)
                                                {
                                                    len = 128 * 128 * (l1 - 128) + 128 * (l2 - 128) + l3;
                                                }
                                                else
                                                {
                                                    byte l4 = reader.ReadByte();
                                                    i += 1;
                                                    ii += 1;
                                                    if (l4 <= 127)
                                                    {
                                                        len = 128 * 128 * 128 * (l1 - 128) + 128 * 128 * (l2 - 128) + 128 * (l3 - 128) + l4;
                                                    }
                                                }
                                            }
                                        }
                                        // Variable-length value end
                                        log += " len=" + len + " ";
                                        for (int iii = 0; iii < len;)
                                        {
                                            log += Convert.ToString(reader.ReadByte(), 16) + " ";
                                            i += 1;
                                            ii += 1;
                                            iii += 1;
                                        }
                                        log += " END";
                                        break;
                                    default:
                                        log += " denied";
                                        break;
                                }
                                break;
                            default:
                                log += "now_running=" + Convert.ToString(running, 16);
                                switch (running / 16)
                                {
                                    case 8:
                                        log += " note_off";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " note_num=" + note_num;
                                        velocity = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        log += " velocity=" + velocity;
                                        EventList[track_now - 1].Add(new int[] { ch_num, deltatime, note_num, 1 , velocity });
                                        break;
                                    case 9:
                                        log += " note_on";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " note_num=" + note_num;
                                        velocity = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        log += " velocity=" + velocity;
                                        EventList[track_now - 1].Add(new int[] { ch_num, deltatime, note_num, 0 , velocity });
                                        break;
                                    case 10:
                                        log += " key_after_touch";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " note_num=" + note_num;
                                        velocity = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        log += " velocity=" + velocity;
                                        break;
                                    case 11:
                                        log += " ctrl_change";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " note_num=" + note_num;
                                        velocity = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        log += " velocity=" + velocity;
                                        break;
                                    case 12:
                                        log += " program_change";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " change_num=" + note_num;
                                        break;
                                    case 13:
                                        log += " chanel_after_touch";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " velocity=" + note_num;
                                        break;
                                    case 14:
                                        log += " pitch_bend";
                                        ch_num = running % 16;
                                        log += " ch_num=" + ch_num;
                                        note_num = t_event;
                                        log += " mean=" + note_num;
                                        velocity = reader.ReadByte();
                                        i += 1;
                                        ii += 1;
                                        log += " range=" + velocity;
                                        break;
                                    case 15:
                                        log += " extra_event=" + running + " ";
                                        switch (running % 16)
                                        {
                                            case 0:
                                                log += Convert.ToString(t_event, 16) + " ";
                                                if (t_event != 247)
                                                    while (true)
                                                    {
                                                        mystery = reader.ReadByte();
                                                        log += Convert.ToString(mystery, 16) + " ";
                                                        i += 1;
                                                        ii += 1;
                                                        if (mystery == 247) break;
                                                    }
                                                log += " END";
                                                break;
                                            case 1:
                                                mystery = t_event;
                                                log += " time=";
                                                log += Convert.ToString(mystery, 16);
                                                break;
                                            case 2:
                                                mystery = t_event;
                                                log += " time1=";
                                                log += Convert.ToString(mystery, 16);
                                                mystery = t_event;
                                                log += " time2=";
                                                log += Convert.ToString(mystery, 16);
                                                break;
                                            case 3:
                                                mystery = t_event;
                                                log += " songnum=";
                                                log += Convert.ToString(mystery, 16);
                                                break;
                                            case 15:
                                                mystery = t_event;
                                                log += " meta_type=";
                                                log += Convert.ToString(mystery, 16);
                                                // Variable-length value begin
                                                int len = 0;
                                                byte l1 = reader.ReadByte();
                                                i += 1;
                                                ii += 1;
                                                if (l1 <= 127)
                                                {
                                                    len = l1;
                                                }
                                                else
                                                {
                                                    byte l2 = reader.ReadByte();
                                                    i += 1;
                                                    ii += 1;
                                                    if (l2 <= 127)
                                                    {
                                                        len = 128 * (l1 - 128) + l2;
                                                    }
                                                    else
                                                    {
                                                        byte l3 = reader.ReadByte();
                                                        i += 1;
                                                        ii += 1;
                                                        if (l3 <= 127)
                                                        {
                                                            len = 128 * 128 * (l1 - 128) + 128 * (l2 - 128) + l3;
                                                        }
                                                        else
                                                        {
                                                            byte l4 = reader.ReadByte();
                                                            i += 1;
                                                            ii += 1;
                                                            if (l4 <= 127)
                                                            {
                                                                len = 128 * 128 * 128 * (l1 - 128) + 128 * 128 * (l2 - 128) + 128 * (l3 - 128) + l4;
                                                            }
                                                        }
                                                    }
                                                }
                                                // Variable-length value end
                                                log += " len=" + len + " ";
                                                for (int iii = 0; iii < len;)
                                                {
                                                    log += Convert.ToString(reader.ReadByte(), 16) + " ";
                                                    i += 1;
                                                    ii += 1;
                                                    iii += 1;
                                                }
                                                log += " END";
                                                break;
                                            default:
                                                log += " denied";
                                                break;
                                        }
                                        break;
                                    default:
                                        log += "now_running...ERROR";
                                        break;
                                }
                                break;
                        }
                    }
                }
            }

            log += "\nLOAD COMPLETED!";

        }
        finally
        {
            reader.Close();
            f.Close();
        }

        for (int i = 0; i < track_now; i++)
        {
            current_time.Add(0);
            e_num.Add(0);
        }
        for (int i = 0; i < track_now; i++)
        {
            int min = 255;
            int max = 0;
            foreach (int[] list in EventList[i])
            {
                if (list[2] < min) min = list[2];
                if (list[2] > max) max = list[2];
            }
            if (min != 0)
                for (int ii = 0; ii < max - min + 1; ii++)
                {
                    OList[i, ii + min] = PList[i].transform.GetChild(ii).gameObject;
                }
        }
    }
    IEnumerator Wait(float sec)
    {
        yield return new WaitForSeconds(sec);
    }
    // Update is called once per frame
    float clip_time = 0;
    [SerializeField]int bpm = 140;
    int sec = 0;
    public bool debugplay;
    void Update()
    {
        float DT = Time.deltaTime;
        clip_time += DT;
        if (debugplay && clip_time > 1)
        {
            au.time = sec + clip_time - 1;
            au.Play();
            sec += 1;
            clip_time -= 1;
        }
        for (int i = 0; i < track_now; i++)
        {
            if (e_num[i] < EventList[i].Count)
            {
                current_time[i] += DT;
                if (current_time[i] - 1 > EventList[i][e_num[i]][1] * 60f / bpm / measure)
                {
                    current_time[i] -= EventList[i][e_num[i]][1] * 60f / bpm / measure;
                    if (EventList[i][e_num[i]][3] == 1)
                    {
                        OList[i, EventList[i][e_num[i]][2]].GetComponent<AnimObjects>().Off();
                    }
                    else
                    {
                        if (EventList[i][e_num[i]][4] == 0)
                        {
                            OList[i, EventList[i][e_num[i]][2]].GetComponent<AnimObjects>().Off();
                        }
                        else
                        {
                            OList[i, EventList[i][e_num[i]][2]].GetComponent<AnimObjects>().On();
                        }
                    }
                    e_num[i] += 1;
                }
            }
        }
    }
}
