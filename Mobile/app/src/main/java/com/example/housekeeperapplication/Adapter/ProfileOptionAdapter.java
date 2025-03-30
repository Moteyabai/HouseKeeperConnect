package com.example.housekeeperapplication.Adapter;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.housekeeperapplication.ProfileOption;
import com.example.housekeeperapplication.R;

import java.util.List;

public class ProfileOptionAdapter extends RecyclerView.Adapter<ProfileOptionAdapter.ViewHolder> {
    private final List<ProfileOption> options;
    private final OnItemClickListener listener;

    public ProfileOptionAdapter(List<ProfileOption> options, OnItemClickListener listener) {
        this.options = options;
        this.listener = listener;
    }

    public interface OnItemClickListener {
        void onItemClick(ProfileOption option);
    }

    @NonNull
    @Override
    public ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.item_profile_option, parent, false);
        return new ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        ProfileOption option = options.get(position);
        holder.icon.setImageResource(option.getIconResId());
        holder.title.setText(option.getTitle());

        holder.itemView.setOnClickListener(v -> listener.onItemClick(option));
    }

    @Override
    public int getItemCount() {
        return options.size();
    }

    public static class ViewHolder extends RecyclerView.ViewHolder {
        ImageView icon;
        TextView title;

        public ViewHolder(View itemView) {
            super(itemView);
            icon = itemView.findViewById(R.id.ivOptionIcon);
            title = itemView.findViewById(R.id.tvOptionTitle);
        }
    }
}