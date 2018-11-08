package com.example.yangJihyun.bubu_main;

import android.graphics.Color;
import android.os.Build;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import com.google.firebase.remoteconfig.FirebaseRemoteConfig;

public class MainActivity extends AppCompatActivity {

    private Button refreshbutton;
    private FirebaseRemoteConfig firebaseRemoteConfig;
    private ImageView imageView;
    int imageIds[] = {R.drawable.grandfa1, R.drawable.grandfa2, R.drawable.grandma1, R.drawable.grandma2};
    int count = imageIds.length;
    int currentIndex = -1;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        firebaseRemoteConfig = FirebaseRemoteConfig.getInstance();
        String splash_background = firebaseRemoteConfig.getString(getString(R.string.rc_color));
        imageView = (ImageView)findViewById(R.id.image);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            getWindow().setStatusBarColor(Color.parseColor(splash_background));
        }
        refreshbutton = (Button)findViewById(R.id.refreshbutton);


        refreshbutton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {

                currentIndex++;
                //  Check If index reaches maximum then reset it
                if (currentIndex == count)
                    currentIndex = 0;

                imageView.setImageResource(imageIds[currentIndex]);

            }
        });


    }



}
