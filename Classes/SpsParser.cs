// Copyright © 2019 Shawn Baker using the MIT License.
using System;

namespace RPiCameraViewer
{
	public class SpsParser
	{
		// instance variables
		private SpsReader reader;
		public int width, height;
		public float fps;
		public int num_units_in_tick = 0;
		public int time_scale = 0;

		public SpsParser(byte[] nal, int len)
		{
			reader = new SpsReader(nal, len);
			reader.SkipBits((nal[2] == 0) ? 40 : 36);

			int frame_crop_left_offset = 0;
			int frame_crop_right_offset = 0;
			int frame_crop_top_offset = 0;
			int frame_crop_bottom_offset = 0;

			int profile_idc = reader.ReadBits(8);
			int constraint_set0_flag = reader.ReadBit();
			int constraint_set1_flag = reader.ReadBit();
			int constraint_set2_flag = reader.ReadBit();
			int constraint_set3_flag = reader.ReadBit();
			int constraint_set4_flag = reader.ReadBit();
			int constraint_set5_flag = reader.ReadBit();
			int reserved_zero_2bits = reader.ReadBits(2);
			int level_idc = reader.ReadBits(8);
			int seq_parameter_set_id = reader.ReadExpGolombCode();

			if (profile_idc == 100 || profile_idc == 110 ||
				profile_idc == 122 || profile_idc == 244 ||
				profile_idc == 44 || profile_idc == 83 ||
				profile_idc == 86 || profile_idc == 118)
			{
				int chroma_format_idc = reader.ReadExpGolombCode();

				if (chroma_format_idc == 3)
				{
					int residual_colour_transform_flag = reader.ReadBit();
				}
				int bit_depth_luma_minus8 = reader.ReadExpGolombCode();
				int bit_depth_chroma_minus8 = reader.ReadExpGolombCode();
				int qpprime_y_zero_transform_bypass_flag = reader.ReadBit();
				int seq_scaling_matrix_present_flag = reader.ReadBit();

				if (seq_scaling_matrix_present_flag != 0)
				{
					int i = 0;
					for (i = 0; i < 8; i++)
					{
						int seq_scaling_list_present_flag = reader.ReadBit();
						if (seq_scaling_list_present_flag != 0)
						{
							int sizeOfScalingList = (i < 6) ? 16 : 64;
							int lastScale = 8;
							int nextScale = 8;
							int j = 0;
							for (j = 0; j < sizeOfScalingList; j++)
							{
								if (nextScale != 0)
								{
									int delta_scale = reader.ReadSignedExpGolombCode();
									nextScale = (lastScale + delta_scale + 256) % 256;
								}
								lastScale = (nextScale == 0) ? lastScale : nextScale;
							}
						}
					}
				}
			}

			int log2_max_frame_num_minus4 = reader.ReadExpGolombCode();
			int pic_order_cnt_type = reader.ReadExpGolombCode();
			if (pic_order_cnt_type == 0)
			{
				int log2_max_pic_order_cnt_lsb_minus4 = reader.ReadExpGolombCode();
			}
			else if (pic_order_cnt_type == 1)
			{
				int delta_pic_order_always_zero_flag = reader.ReadBit();
				int offset_for_non_ref_pic = reader.ReadSignedExpGolombCode();
				int offset_for_top_to_bottom_field = reader.ReadSignedExpGolombCode();
				int num_ref_frames_in_pic_order_cnt_cycle = reader.ReadExpGolombCode();
				for (int i = 0; i < num_ref_frames_in_pic_order_cnt_cycle; i++)
				{
					reader.ReadSignedExpGolombCode();
				}
			}
			int max_num_ref_frames = reader.ReadExpGolombCode();
			int gaps_in_frame_num_value_allowed_flag = reader.ReadBit();
			int pic_width_in_mbs_minus1 = reader.ReadExpGolombCode();
			int pic_height_in_map_units_minus1 = reader.ReadExpGolombCode();
			int frame_mbs_only_flag = reader.ReadBit();
			if (frame_mbs_only_flag == 0)
			{
				int mb_adaptive_frame_field_flag = reader.ReadBit();
			}
			int direct_8x8_inference_flag = reader.ReadBit();
			int frame_cropping_flag = reader.ReadBit();
			if (frame_cropping_flag != 0)
			{
				frame_crop_left_offset = reader.ReadExpGolombCode();
				frame_crop_right_offset = reader.ReadExpGolombCode();
				frame_crop_top_offset = reader.ReadExpGolombCode();
				frame_crop_bottom_offset = reader.ReadExpGolombCode();
			}
			int vui_parameters_present_flag = reader.ReadBit();
			if (vui_parameters_present_flag != 0)
			{
				int aspect_ratio_info_present_flag = reader.ReadBit();
				if (aspect_ratio_info_present_flag != 0)
				{
					int aspect_ratio = reader.ReadBits(8);
					/*
					int aspect_ratio = AspectRatio.fromValue((int) reader.readBits(8));
					if (aspect_ratio == AspectRatio.Extended_SAR)
					{
						int sar_width = reader.readBits(16);
						int sar_height = reader.readBits(16);
					}
					*/
				}
				int overscan_info_present_flag = reader.ReadBit();
				if (overscan_info_present_flag != 0)
				{
					int overscan_appropriate_flag = reader.ReadBit();
				}
				int video_signal_type_present_flag = reader.ReadBit();
				if (video_signal_type_present_flag != 0)
				{
					int video_format = (int)reader.ReadBits(3);
					int video_full_range_flag = reader.ReadBit();
					int colour_description_present_flag = reader.ReadBit();
					if (colour_description_present_flag != 0)
					{
						int colour_primaries = (int)reader.ReadBits(8);
						int transfer_characteristics = (int)reader.ReadBits(8);
						int matrix_coefficients = (int)reader.ReadBits(8);
					}
				}
				int chroma_loc_info_present_flag = reader.ReadBit();
				if (chroma_loc_info_present_flag != 0)
				{
					int chroma_sample_loc_type_top_field = reader.ReadExpGolombCode();
					int chroma_sample_loc_type_bottom_field = reader.ReadExpGolombCode();
				}
				int timing_info_present_flag = reader.ReadBit();
				if (timing_info_present_flag != 0)
				{
					num_units_in_tick = (int)reader.ReadBits(32);
					time_scale = (int)reader.ReadBits(32);
					int fixed_frame_rate_flag = reader.ReadBit();
				}
				int nal_hrd_parameters_present_flag = reader.ReadBit();
				if (nal_hrd_parameters_present_flag != 0)
				{
					ReadHRDParameters();
				}
				int vcl_hrd_parameters_present_flag = reader.ReadBit();
				if (vcl_hrd_parameters_present_flag != 0)
				{
					ReadHRDParameters();
				}
				if (nal_hrd_parameters_present_flag != 0 || vcl_hrd_parameters_present_flag != 0)
				{
					int low_delay_hrd_flag = reader.ReadBit();
				}
				int pic_struct_present_flag = reader.ReadBit();
				int bitstream_restriction_flag = reader.ReadBit();
				if (bitstream_restriction_flag != 0)
				{
					int motion_vectors_over_pic_boundaries_flag = reader.ReadBit();
					int max_bytes_per_pic_denom = reader.ReadExpGolombCode();
					int max_bits_per_mb_denom = reader.ReadExpGolombCode();
					int log2_max_mv_length_horizontal = reader.ReadExpGolombCode();
					int log2_max_mv_length_vertical = reader.ReadExpGolombCode();
					int num_reorder_frames = reader.ReadExpGolombCode();
					int max_dec_frame_buffering = reader.ReadExpGolombCode();
				}
			}

			width = ((pic_width_in_mbs_minus1 + 1) * 16) - frame_crop_bottom_offset * 2 - frame_crop_top_offset * 2;
			height = ((2 - frame_mbs_only_flag) * (pic_height_in_map_units_minus1 + 1) * 16) - (frame_crop_right_offset * 2) - (frame_crop_left_offset * 2);
			if (time_scale != 0)
			{
				fps = (float)num_units_in_tick / time_scale / 2;
				/*
				if (nuit_field_based_flag)
				{
					fps /= 2;
				}
				*/
			}
		}

		private void ReadHRDParameters()
		{
			int cpb_cnt_minus1 = reader.ReadExpGolombCode();
			int bit_rate_scale = (int)reader.ReadBits(4);
			int cpb_size_scale = (int)reader.ReadBits(4);
			//int bit_rate_value_minus1 = new int[cpb_cnt_minus1 + 1];
			//int cpb_size_value_minus1 = new int[cpb_cnt_minus1 + 1];
			//int cbr_flag = new boolean[cpb_cnt_minus1 + 1];

			for (int SchedSelIdx = 0; SchedSelIdx <= cpb_cnt_minus1; SchedSelIdx++)
			{
				/*int bit_rate_value_minus1[SchedSelIdx] = */
				reader.ReadExpGolombCode();
				/*int cpb_size_value_minus1[SchedSelIdx] = */
				reader.ReadExpGolombCode();
				/*int cbr_flag[SchedSelIdx] = */
				reader.ReadBit();
			}
			int initial_cpb_removal_delay_length_minus1 = (int)reader.ReadBits(5);
			int cpb_removal_delay_length_minus1 = (int)reader.ReadBits(5);
			int dpb_output_delay_length_minus1 = (int)reader.ReadBits(5);
			int time_offset_length = (int)reader.ReadBits(5);
		}
	}
}
